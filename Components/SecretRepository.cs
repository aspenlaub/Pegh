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

        public void Set<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult> {
            var value = ValueOrDefault(secret);
            var xml = vComponentProvider.XmlSerializer.Serialize(value);
            var fileName = FileName(secret);
            File.WriteAllText(fileName, xml);
        }

        private TResult ValueOrDefault<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult> {
            if (!Values.ContainsKey(secret.Guid)) {
                Values[secret.Guid] = secret.DefaultValue.Clone();
            }
            var value = Values[secret.Guid] as TResult;
            return value;
        }

        public TResult Get<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult> {
            TResult valueOrDefault;

            var fileName = FileName(secret);
            if (!File.Exists(fileName)) {
                valueOrDefault = ValueOrDefault(secret);
                Values[secret.Guid] = valueOrDefault;
                var shouldDefaultSecretsBeStored = ValueOrDefault(SecretShouldDefaultSecretsBeStored);
                if (!shouldDefaultSecretsBeStored.AutomaticallySaveDefaulSecretIfAbsent) { return valueOrDefault; }

                Set(secret);
                return valueOrDefault;
            }

            if (Values.ContainsKey(secret.Guid)) {
                valueOrDefault = ValueOrDefault(secret);
                return valueOrDefault;
            }

            valueOrDefault = vComponentProvider.XmlDeserializer.Deserialize<TResult>(File.ReadAllText(fileName));
            Values[secret.Guid] = valueOrDefault;
            return valueOrDefault;
        }

        public TResult Get<TArgument, TResult>(IPowershellSecret<TArgument, TResult> secret, TArgument arg) where TResult : class {
            var powershellFunction = ValueOrDefault(secret);
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

        public void Reset<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult> {
            if (Values.ContainsKey(secret.Guid)) {
                Values.Remove(secret.Guid);
            }

            var fileName = FileName(secret);
            if (!File.Exists(fileName)) { return; }

            File.Delete(fileName);
        }

        public bool Exists<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult> {
            return File.Exists(FileName(secret));
        }

        private string FileName(IGuid secret) {
            return vComponentProvider.PeghEnvironment.RootWorkFolder + @"\SecretRepository\" + secret.Guid + @".xml";
        }
    }
}
