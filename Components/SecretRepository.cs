using System;
using System.IO;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class SecretRepository : ISecretRepository {
        private readonly IComponentProvider vComponentProvider;

        public SecretRepository(IComponentProvider componentProvider) {
            vComponentProvider = componentProvider;
        }

        public void Set<TResult>(ISecret<TResult> secret) {
            var xml = vComponentProvider.XmlSerializer.Serialize(secret.Value);
            var fileName = FileName(secret);
            File.WriteAllText(fileName, xml);
        }

        public void Get<TResult>(ISecret<TResult> secret) {
            if (secret.SecretType != SecretTypes.Fixed) {
                secret.Value = secret.DefaultValue;
                return;
            }

            var fileName = FileName(secret);
            if (!File.Exists(fileName)) {
                secret.Value = secret.DefaultValue;
                return;
            }

            secret.Value = vComponentProvider.XmlDeserializer.Deserialize<TResult>(File.ReadAllText(fileName));
        }

        public void Get<TArgument, TResult>(ISecret<TResult> secret, TArgument arg) {
            if (secret.SecretType == SecretTypes.Script) {
                throw new NotImplementedException();
            }

            secret.Value = secret.DefaultValue;
        }

        public void Reset<TResult>(ISecret<TResult> secret) {
            var fileName = FileName(secret);
            if (!File.Exists(fileName)) { return; }

            File.Delete(fileName);
        }

        public bool Exists<TResult>(ISecret<TResult> secret) {
            return File.Exists(FileName(secret));
        }

        private string FileName(IGuid secret) {
            return vComponentProvider.PeghEnvironment.RootWorkFolder + @"\SecretRepository\" + secret.Guid + @".xml";
        }
    }
}
