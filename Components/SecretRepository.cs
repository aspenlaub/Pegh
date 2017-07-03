using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class SecretRepository : ISecretRepository {
        private readonly IComponentProvider vComponentProvider;
        internal readonly Dictionary<string, object> Values;
        internal readonly SecretShouldDefaultSecretsBeStored SecretShouldDefaultSecretsBeStored;

        public SecretRepository(IComponentProvider componentProvider) {
            vComponentProvider = componentProvider;
            Values = new Dictionary<string, object>();
            SecretShouldDefaultSecretsBeStored = new SecretShouldDefaultSecretsBeStored();
            Get(SecretShouldDefaultSecretsBeStored);
        }

        public void Set<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>, new() {
            var valueOrDefault = ValueOrDefault(secret);
            var xml = vComponentProvider.XmlSerializer.Serialize(valueOrDefault);
            var fileName = FileName(secret);
            File.WriteAllText(fileName, xml);
            Values[secret.Guid] = valueOrDefault;
        }

        internal TResult ValueOrDefault<TResult>(ISecret<TResult> secret)
                where TResult : class, ISecretResult<TResult> {
            if (Values.ContainsKey(secret.Guid)) {
                return Values[secret.Guid] as TResult;
            }

            var fileName = FileName(secret);
            if (File.Exists(fileName)) {
                Values[secret.Guid] = vComponentProvider.XmlDeserializer.Deserialize<TResult>(File.ReadAllText(fileName));
                return (TResult) Values[secret.Guid];
            }

            var clone = secret.DefaultValue.Clone();
            Values[secret.Guid] = clone;
            return clone;
        }

        public TResult Get<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>, new() {
            TResult valueOrDefault;

            SaveSample(secret);

            var fileName = FileName(secret);
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

            valueOrDefault = vComponentProvider.XmlDeserializer.Deserialize<TResult>(File.ReadAllText(fileName));
            Values[secret.Guid] = valueOrDefault;
            return valueOrDefault;
        }

        public TResult ExecutePowershellFunction<TArgument, TResult>(IPowershellFunction<TArgument, TResult> powershellFunction, TArgument arg) where TResult : class {
            var script = "Param(\r\n"
                         + "\t[Parameter(Mandatory=$true)]\r\n"
                         + "\t$secretArgument\r\n"
                         + ")\r\n\r\n"
                         + "Add-Type -Path \""
                         + typeof(PowershellFunctionResult).Assembly.Location
                         + "\"\r\n\r\n"
                         + powershellFunction.Script
                         + "\r\n\r\n"
                         + powershellFunction.FunctionName + "($secretArgument)\r\n";
            using (var powerShellInstance = PowerShell.Create()) {
                powerShellInstance.AddScript(script);
                powerShellInstance.AddParameter("secretArgument", arg);
                var invokeResults = powerShellInstance.Invoke();
                if (powerShellInstance.Streams.Error.Count > 0 || invokeResults.Count != 1) {
                    return null;
                }

                var invokeResult = invokeResults[0].BaseObject as PowershellFunctionResult;
                return invokeResult?.Result as TResult;
            }
        }

        public void Reset(IGuid secret) {
            if (Values.ContainsKey(secret.Guid)) {
                Values.Remove(secret.Guid);
            }

            var fileName = FileName(secret);
            if (!File.Exists(fileName)) { return; }

            File.Delete(fileName);
        }

        public bool Exists(IGuid secret) {
            return File.Exists(FileName(secret));
        }

        private string FileName(IGuid secret) {
            return FileName(secret, false);
        }

        private string FileName(IGuid secret, bool sample) {
            return vComponentProvider.PeghEnvironment.RootWorkFolder + (sample ? @"\SecretSamples\" : @"\SecretRepository\") + secret.Guid + @".xml";
        }

        public void SaveSample<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>, new() {
            var fileName = FileName(secret, true);
            if (File.Exists(fileName)) { return; }

            var xml = vComponentProvider.XmlSerializer.Serialize(secret.DefaultValue);
            File.WriteAllText(fileName, xml);
        }
    }
}
