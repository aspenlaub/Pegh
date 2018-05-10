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
        internal SecretShouldDefaultSecretsBeStored SecretShouldDefaultSecretsBeStored;

        public bool IsUserPresent { get; internal set; }
        public string PassphraseIfUserIsNotPresent { get; internal set; }

        public SecretRepository(IComponentProvider componentProvider) {
            ComponentProvider = componentProvider;
            Values = new Dictionary<string, object>();
            IsUserPresent = true;
            var folder = ComponentProvider.PeghEnvironment.RootWorkFolder + @"\SecretSamples\";
            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
            folder = ComponentProvider.PeghEnvironment.RootWorkFolder + @"\SecretRepository\";
            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
        }

        public void Set<TResult>(ISecret<TResult> secret, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new() {
            var valueOrDefault = ValueOrDefault(secret, errorsAndInfos);
            var xml = ComponentProvider.XmlSerializer.Serialize(valueOrDefault);
            var encrypted = secret is IEncryptedSecret<TResult>;
            WriteToFile(secret, xml, false, encrypted, errorsAndInfos);
            Values[secret.Guid] = valueOrDefault;
        }

        internal TResult ValueOrDefault<TResult>(ISecret<TResult> secret, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new() {
            if (Values.ContainsKey(secret.Guid)) {
                return Values[secret.Guid] as TResult;
            }

            var encrypted = secret is IEncryptedSecret<TResult>;
            if (File.Exists(FileName(secret, false, encrypted))) {
                var xml = ReadFromFile(secret, false, encrypted, errorsAndInfos);
                Values[secret.Guid] = ComponentProvider.XmlDeserializer.Deserialize<TResult>(xml);
                return (TResult) Values[secret.Guid];
            }

            var clone = secret.DefaultValue.Clone();
            Values[secret.Guid] = clone;
            return clone;
        }

        public TResult Get<TResult>(ISecret<TResult> secret, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new() {
            TResult valueOrDefault;

            SaveSample(secret, false);

            var encrypted = secret is IEncryptedSecret<TResult>;
            var fileName = FileName(secret, false, encrypted);
            if (!File.Exists(fileName)) {
                if (SecretShouldDefaultSecretsBeStored == null) {
                    SecretShouldDefaultSecretsBeStored = new SecretShouldDefaultSecretsBeStored();
                    Get(SecretShouldDefaultSecretsBeStored, errorsAndInfos);
                }
                var shouldDefaultSecretsBeStored = ValueOrDefault(SecretShouldDefaultSecretsBeStored, errorsAndInfos);
                if (!shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent) {
                    SaveSample(secret, true);
                    var defaultFileName = FileName(secret, true, encrypted);
                    errorsAndInfos.Errors.Add(string.Format(Properties.Resources.PleaseLoadSecretSampleAdjustAndThenSaveAs, defaultFileName, fileName));
                    return null;
                }

                Set(secret, errorsAndInfos);
                return ValueOrDefault(secret, errorsAndInfos);
            }

            if (Values.ContainsKey(secret.Guid)) {
                valueOrDefault = ValueOrDefault(secret, errorsAndInfos);
                return valueOrDefault;
            }

            var xml = ReadFromFile(secret, false, encrypted, errorsAndInfos);
            if (string.IsNullOrEmpty(xml)) { return null; }

            valueOrDefault = ComponentProvider.XmlDeserializer.Deserialize<TResult>(xml);
            if (!IsGenericType(valueOrDefault.GetType())) {
                foreach (var property in valueOrDefault.GetType().GetProperties().Where(p => p.GetValue(valueOrDefault) == null)) {
                    SaveSample(secret, true);
                    var defaultFileName = FileName(secret, true, encrypted);
                    errorsAndInfos.Errors.Add(string.Format(Properties.Resources.AddedPropertyNotFoundInLoadedSecret, property.Name, fileName, defaultFileName));
                }
            }

            Values[secret.Guid] = valueOrDefault;
            return valueOrDefault;
        }

        protected static bool IsGenericType(Type t) {
            while (t != null) {
                if (t.IsGenericType) { return true; }

                t = t.BaseType;
            }

            return false;
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

        internal void SaveSample<TResult>(ISecret<TResult> secret, bool update) where TResult : class, ISecretResult<TResult>, new() {
            var encrypted = secret is IEncryptedSecret<TResult>;
            var fileName = FileName(secret, true, encrypted);
            if (File.Exists(fileName) && !update) { return; }

            var xml = ComponentProvider.XmlSerializer.Serialize(secret.DefaultValue);
            if (File.Exists(fileName) && File.ReadAllText(fileName) == xml) { return; }

            File.WriteAllText(fileName, xml);
            File.WriteAllText(fileName.Replace(".xml", ".xsd"), ComponentProvider.XmlSchemer.Create(typeof(TResult)));
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        internal void WriteToFile<TResult>(ISecret<TResult> secret, string xml, bool sample, bool encrypted, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new() {
            if (!ComponentProvider.XmlSchemer.Valid(secret.Guid, xml, typeof(TResult), errorsAndInfos)) { return; }

            var fileName = FileName(secret, sample, encrypted);
            if (!encrypted) {
                File.WriteAllText(fileName, xml);
                File.WriteAllText(fileName.Replace(".xml", ".xsd"), ComponentProvider.XmlSchemer.Create(typeof(TResult)));
                return;
            }

            var disguisedPassphrase = GetDisguisedPassphrase(errorsAndInfos);
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
        private string ReadFromFile<TResult>(ISecret<TResult> secret, bool sample, bool encrypted, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new() {
            var xml = "";

            var fileName = FileName(secret, sample, encrypted);
            if (!encrypted) {
                xml = File.ReadAllText(fileName);
                return ComponentProvider.XmlSchemer.Valid(secret.Guid, xml, typeof(TResult), errorsAndInfos) ? xml : "";
            }

            var disguisedPassphrase = GetDisguisedPassphrase(errorsAndInfos);
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

            return ComponentProvider.XmlSchemer.Valid(secret.Guid, xml, typeof(TResult), errorsAndInfos) ? xml : "";
        }

        private string FileName(IGuid secret, bool sample, bool encrypted) {
            return ComponentProvider.PeghEnvironment.RootWorkFolder + (sample ? @"\SecretSamples\" : @"\SecretRepository\") + secret.Guid + (encrypted ? @".7zip" : @".xml");
        }

        private string GetDisguisedPassphrase(IErrorsAndInfos errorsAndInfos) {
            var passphraseFunction = Get(new SecretPassphraseFunction(), errorsAndInfos);
            var passphraseFunctionArgument = new SecretPassphraseFunctionArgument { IsUserPresent = IsUserPresent, PassphraseIfUserIsNotPresent = PassphraseIfUserIsNotPresent };
            var passphrase = ExecutePowershellFunction(passphraseFunction, passphraseFunctionArgument);
            return string.IsNullOrEmpty(passphrase) ? "" : ComponentProvider.Disguiser.Disguise(passphrase, errorsAndInfos);
        }
    }
}
