using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Ionic.Zip;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class SecretRepository : ISecretRepository {
        protected readonly IComponentProvider ComponentProvider;
        internal readonly Dictionary<string, object> Values;
        internal readonly SecretShouldDefaultSecretsBeStored SecretShouldDefaultSecretsBeStored;

        public bool IsUserPresent { get; internal set; }
        public string PassphraseIfUserIsNotPresent { get; internal set; }

        public SecretRepository(IComponentProvider componentProvider) {
            ComponentProvider = componentProvider;
            Values = new Dictionary<string, object>();
            SecretShouldDefaultSecretsBeStored = new SecretShouldDefaultSecretsBeStored();
            IsUserPresent = true;
            Get(SecretShouldDefaultSecretsBeStored);
        }

        public void Set<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>, new() {
            var valueOrDefault = ValueOrDefault(secret);
            var xml = ComponentProvider.XmlSerializer.Serialize(valueOrDefault);
            var encrypted = secret is IEncryptedSecret<TResult>;
            WriteToFile(secret, xml, false, encrypted);
            Values[secret.Guid] = valueOrDefault;
        }

        internal TResult ValueOrDefault<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>, new() {
            if (Values.ContainsKey(secret.Guid)) {
                return Values[secret.Guid] as TResult;
            }

            var encrypted = secret is IEncryptedSecret<TResult>;
            if (File.Exists(FileName(secret, false, encrypted))) {
                var xml = ReadFromFile(secret, false, encrypted);
                Values[secret.Guid] = ComponentProvider.XmlDeserializer.Deserialize<TResult>(xml);
                return (TResult) Values[secret.Guid];
            }

            var clone = secret.DefaultValue.Clone();
            Values[secret.Guid] = clone;
            return clone;
        }

        public TResult Get<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>, new() {
            TResult valueOrDefault;

            SaveSample(secret);

            var encrypted = secret is IEncryptedSecret<TResult>;
            var fileName = FileName(secret, false, encrypted);
            if (!File.Exists(fileName)) {
                var shouldDefaultSecretsBeStored = ValueOrDefault(SecretShouldDefaultSecretsBeStored);
                if (!shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent) { return null; }

                Set(secret);
                return ValueOrDefault(secret);
            }

            if (Values.ContainsKey(secret.Guid)) {
                valueOrDefault = ValueOrDefault(secret);
                return valueOrDefault;
            }

            var xml = ReadFromFile(secret, false, encrypted);
            if (string.IsNullOrEmpty(xml)) { return null; }

            valueOrDefault = ComponentProvider.XmlDeserializer.Deserialize<TResult>(xml);
            Values[secret.Guid] = valueOrDefault;
            return valueOrDefault;
        }

        public TResult ExecutePowershellFunction<TArgument, TResult>(IPowershellFunction<TArgument, TResult> powershellFunction, TArgument arg) where TResult : class {
            return ComponentProvider.PowershellExecuter.ExecutePowershellFunction(powershellFunction, arg);
        }

        internal void Reset(IGuid secret, bool encrypted) {
            if (Values.ContainsKey(secret.Guid)) {
                Values.Remove(secret.Guid);
            }

            var fileName = FileName(secret, false, encrypted);
            if (!File.Exists(fileName)) { return; }

            File.Delete(fileName);
        }

        internal bool Exists(IGuid secret, bool encrypted) {
            return File.Exists(FileName(secret, false, encrypted));
        }

        internal void SaveSample<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>, new() {
            var encrypted = secret is IEncryptedSecret<TResult>;
            var fileName = FileName(secret, true, encrypted);
            if (File.Exists(fileName)) { return; }

            var xml = ComponentProvider.XmlSerializer.Serialize(secret.DefaultValue);
            File.WriteAllText(fileName, xml);
            File.WriteAllText(fileName.Replace(".xml", ".xsd"), ComponentProvider.XmlSchemer.Create(typeof(TResult)));
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        internal void WriteToFile<TResult>(ISecret<TResult> secret, string xml, bool sample, bool encrypted) where TResult : class, ISecretResult<TResult>, new() {
            if (!ComponentProvider.XmlSchemer.Valid(xml, typeof(TResult))) { return; }

            var fileName = FileName(secret, sample, encrypted);
            if (!encrypted) {
                File.WriteAllText(fileName, xml);
                File.WriteAllText(fileName.Replace(".xml", ".xsd"), ComponentProvider.XmlSchemer.Create(typeof(TResult)));
                return;
            }

            var disguisedPassphrase = GetDisguisedPassphrase();
            if (string.IsNullOrEmpty(disguisedPassphrase)) { return; }

            var unencryptedFileName = FileName(secret, sample, false);
            unencryptedFileName = unencryptedFileName.Substring(unencryptedFileName.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            using (var zipFile = new ZipFile(fileName) { Encryption = EncryptionAlgorithm.WinZipAes256, Password = disguisedPassphrase }) {
                zipFile.AddEntry(unencryptedFileName, xml);
                zipFile.Save();
                zipFile.Dispose();
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private string ReadFromFile<TResult>(ISecret<TResult> secret, bool sample, bool encrypted) where TResult : class, ISecretResult<TResult>, new() {
            var xml = "";

            var fileName = FileName(secret, sample, encrypted);
            if (!encrypted) {
                xml = File.ReadAllText(fileName);
                return ComponentProvider.XmlSchemer.Valid(xml, typeof(TResult)) ? xml : "";
            }

            var disguisedPassphrase = GetDisguisedPassphrase();
            if (string.IsNullOrEmpty(disguisedPassphrase)) { return ""; }

            var unencryptedFileName = FileName(secret, sample, false);
            unencryptedFileName = unencryptedFileName.Substring(unencryptedFileName.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            using (var zipFile = ZipFile.Read(fileName)) {
                var zipEntry = zipFile.FirstOrDefault(f => f.FileName == unencryptedFileName);
                if (zipEntry != null) {
                    using (var stream = new MemoryStream()) {
                        try {
                            zipEntry.ExtractWithPassword(stream, disguisedPassphrase);
                            xml = Encoding.UTF8.GetString(stream.ToArray());
                        }
                        catch {
                            // ignored
                        }
                    }
                }
                zipFile.Dispose();
            }

            return ComponentProvider.XmlSchemer.Valid(xml, typeof(TResult)) ? xml : "";
        }

        private string FileName(IGuid secret, bool sample, bool encrypted) {
            return ComponentProvider.PeghEnvironment.RootWorkFolder + (sample ? @"\SecretSamples\" : @"\SecretRepository\") + secret.Guid + (encrypted ? @".7zip" : @".xml");
        }

        private string GetDisguisedPassphrase() {
            var passphraseFunction = Get(new SecretPassphraseFunction());
            var passphraseFunctionArgument = new SecretPassphraseFunctionArgument { IsUserPresent = IsUserPresent, PassphraseIfUserIsNotPresent = PassphraseIfUserIsNotPresent };
            var passphrase = ExecutePowershellFunction(passphraseFunction, passphraseFunctionArgument);
            return string.IsNullOrEmpty(passphrase) ? "" : ComponentProvider.Disguiser.Disguise(passphrase);
        }
    }
}
