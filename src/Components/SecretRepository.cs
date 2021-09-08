using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

[assembly: InternalsVisibleTo("Aspenlaub.Net.GitHub.CSharp.Pegh.Test")]
namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class SecretRepository : ISecretRepository {
        internal readonly Dictionary<string, object> Values;
        internal SecretShouldDefaultSecretsBeStored SecretShouldDefaultSecretsBeStored;

        public ICsArgumentPrompter CsArgumentPrompter { get; }

        protected readonly ICsLambdaCompiler CsLambdaCompiler;
        protected readonly IDisguiser Disguiser;
        protected readonly IPeghEnvironment PeghEnvironment;
        protected readonly IXmlDeserializer XmlDeserializer;
        protected readonly IXmlSchemer XmlSchemer;
        protected readonly IXmlSerializer XmlSerializer;

        public SecretRepository(ICsArgumentPrompter csArgumentPrompter, ICsLambdaCompiler csLambdaCompiler, IDisguiser disguiser, IPeghEnvironment peghEnvironment, IXmlDeserializer xmlDeserializer, IXmlSerializer xmlSerializer, IXmlSchemer xmlSchemer) {
            CsArgumentPrompter = csArgumentPrompter;
            CsLambdaCompiler = csLambdaCompiler;
            Disguiser = disguiser;
            PeghEnvironment = peghEnvironment;
            XmlDeserializer = xmlDeserializer;
            XmlSchemer = xmlSchemer;
            XmlSerializer = xmlSerializer;
            Values = new Dictionary<string, object>();
            var folder = PeghEnvironment.RootWorkFolder + @"\SecretSamples\";
            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
            folder = PeghEnvironment.RootWorkFolder + @"\SecretRepository\";
            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
        }

        public async Task SetAsync<TResult>(ISecret<TResult> secret, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new() {
            var valueOrDefault = await ValueOrDefaultAsync(secret, errorsAndInfos);
            var xml = XmlSerializer.Serialize(valueOrDefault);
            var encrypted = secret is IEncryptedSecret<TResult>;
            await WriteToFileAsync(secret, xml, false, encrypted, errorsAndInfos);
            Values[secret.Guid] = valueOrDefault;
        }

        internal async Task<TResult> ValueOrDefaultAsync<TResult>(ISecret<TResult> secret, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new() {
            if (Values.ContainsKey(secret.Guid)) {
                return Values[secret.Guid] as TResult;
            }

            var encrypted = secret is IEncryptedSecret<TResult>;
            if (File.Exists(FileName(secret, false, encrypted))) {
                var xml = await ReadFromFileAsync(secret, false, encrypted, errorsAndInfos);
                Values[secret.Guid] = XmlDeserializer.Deserialize<TResult>(xml);
                return (TResult) Values[secret.Guid];
            }

            var clone = secret.DefaultValue.Clone();
            Values[secret.Guid] = clone;
            return clone;
        }

        public async Task<TResult> GetAsync<TResult>(ISecret<TResult> secret, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new() {
            TResult valueOrDefault;

            SaveSample(secret, false);

            var encrypted = secret is IEncryptedSecret<TResult>;
            var fileName = FileName(secret, false, encrypted);
            if (!File.Exists(fileName)) {
                if (SecretShouldDefaultSecretsBeStored == null) {
                    SecretShouldDefaultSecretsBeStored = new SecretShouldDefaultSecretsBeStored();
                    await GetAsync(SecretShouldDefaultSecretsBeStored, errorsAndInfos);
                }
                var shouldDefaultSecretsBeStored = await ValueOrDefaultAsync(SecretShouldDefaultSecretsBeStored, errorsAndInfos);
                if (!shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent) {
                    SaveSample(secret, true);
                    var defaultFileName = FileName(secret, true, encrypted);
                    errorsAndInfos.Errors.Add(string.Format(Properties.Resources.PleaseLoadSecretSampleAdjustAndThenSaveAs, defaultFileName, fileName));
                    return null;
                }

                await SetAsync(secret, errorsAndInfos);
                return await ValueOrDefaultAsync(secret, errorsAndInfos);
            }

            if (Values.ContainsKey(secret.Guid)) {
                valueOrDefault = await ValueOrDefaultAsync(secret, errorsAndInfos);
                return valueOrDefault;
            }

            var xml = await ReadFromFileAsync(secret, false, encrypted, errorsAndInfos);
            if (string.IsNullOrEmpty(xml)) { return null; }

            valueOrDefault = XmlDeserializer.Deserialize<TResult>(xml);
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

        public async Task<Func<TArgument, TResult>> CompileCsLambdaAsync<TArgument, TResult>(ICsLambda csLambda) {
            return await CsLambdaCompiler.CompileCsLambdaAsync<TArgument, TResult>(csLambda);
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

            var xml = XmlSerializer.Serialize(secret.DefaultValue);
            if (File.Exists(fileName) && File.ReadAllText(fileName) == xml) { return; }

            File.WriteAllText(fileName, xml);
            File.WriteAllText(fileName.Replace(".xml", ".xsd"), XmlSchemer.Create(typeof(TResult)));
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        internal async Task WriteToFileAsync<TResult>(ISecret<TResult> secret, string xml, bool sample, bool encrypted, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new() {
            if (!XmlSchemer.Valid(secret.Guid, xml, typeof(TResult), errorsAndInfos)) { return; }

            var fileName = FileName(secret, sample, encrypted);
            if (!encrypted) {
                await File.WriteAllTextAsync(fileName, xml);
                await File.WriteAllTextAsync(fileName.Replace(".xml", ".xsd"), XmlSchemer.Create(typeof(TResult)));
                return;
            }

            var disguisedPassphrase = await GetDisguisedPassphraseAsync(errorsAndInfos);
            if (string.IsNullOrEmpty(disguisedPassphrase)) { return; }

            var unencryptedFileName = FileName(secret, sample, false);
            unencryptedFileName = unencryptedFileName[(unencryptedFileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)..];
            await using var fileStream = File.Create(fileName);
            await using var zipStream = new ZipOutputStream(fileStream);
            zipStream.SetLevel(9);
            zipStream.Password = disguisedPassphrase;
            var newEntry = new ZipEntry(unencryptedFileName) {
                DateTime = DateTime.Now,
                Size = xml.Length
                // AESKeySize = 256
            };
            zipStream.PutNextEntry(newEntry);
            var buffer = new byte[4096];
            await using (var streamReader = new MemoryStream(Encoding.UTF8.GetBytes(xml))) {
                StreamUtils.Copy(streamReader, zipStream, buffer);
            }
            zipStream.CloseEntry();
            zipStream.IsStreamOwner = true;
            zipStream.Close();
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private async Task<string> ReadFromFileAsync<TResult>(ISecret<TResult> secret, bool sample, bool encrypted, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new() {
            var xml = "";

            var fileName = FileName(secret, sample, encrypted);
            if (!encrypted) {
                xml = await File.ReadAllTextAsync(fileName);
                return XmlSchemer.Valid(secret.Guid, xml, typeof(TResult), errorsAndInfos) ? xml : "";
            }

            var disguisedPassphrase = await GetDisguisedPassphraseAsync(errorsAndInfos);
            if (string.IsNullOrEmpty(disguisedPassphrase)) { return ""; }

            var unencryptedFileName = FileName(secret, sample, false);
            unencryptedFileName = unencryptedFileName[(unencryptedFileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)..];
            await using (var zipStream = new FileStream(fileName, FileMode.Open)) {
                var zipInputStream = new ZipInputStream(zipStream) { Password = disguisedPassphrase };
                var zipEntry = zipInputStream.GetNextEntry();
                while (zipEntry != null) {
                    if (zipEntry.Name != unencryptedFileName) {
                        zipEntry = zipInputStream.GetNextEntry();
                        continue;
                    }

                    var buffer = new byte[4096];
                    await using (var stream = new MemoryStream()) {
                        StreamUtils.Copy(zipInputStream, stream, buffer);
                        xml = Encoding.UTF8.GetString(stream.ToArray());
                    }

                    zipEntry = null;
                }
            }

            return XmlSchemer.Valid(secret.Guid, xml, typeof(TResult), errorsAndInfos) ? xml : "";
        }


        public string FileName(IGuid secret, bool sample, bool encrypted) {
            return PeghEnvironment.RootWorkFolder + (sample ? @"\SecretSamples\" : @"\SecretRepository\") + secret.Guid + (encrypted ? @".7zip" : @".xml");
        }

        private async Task<string> GetDisguisedPassphraseAsync(IErrorsAndInfos errorsAndInfos) {
            if (CsArgumentPrompter == null) {
                throw new NullReferenceException( $"You are attempting to use a secret that requires user interaction. Please provide a {nameof(ICsArgumentPrompter)}");
            }

            var passphrase = CsArgumentPrompter.PromptForArgument("passPhrase", "Please enter the required passphrase");
            return string.IsNullOrEmpty(passphrase) ? "" : await Disguiser.Disguise(this, passphrase, errorsAndInfos);
        }
    }
}
