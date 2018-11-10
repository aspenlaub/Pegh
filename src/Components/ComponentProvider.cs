using System;
using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class ComponentProvider : IComponentProvider {
        private Dictionary<Type, object> DefaultComponents { get; }

        public ComponentProvider() {
            DefaultComponents = new Dictionary<Type, object>();
        }

        private T DefaultComponent<T, T2>() where T : class where T2 : T, new() {
            if (!DefaultComponents.ContainsKey(typeof(T))) {
                DefaultComponents[typeof(T)] = new T2();
            }
            return (T)DefaultComponents[typeof(T)];
        }

        private T DefaultComponent<T, T2>(Func<T2> constructor) where T : class where T2 : T {
            if (!DefaultComponents.ContainsKey(typeof(T))) {
                DefaultComponents[typeof(T)] = constructor();
            }
            return (T)DefaultComponents[typeof(T)];
        }

        public ICsLambdaCompiler CsLambdaCompiler => DefaultComponent<ICsLambdaCompiler, CsLambdaCompiler>();
        public IDisguiser Disguiser => DefaultComponent<IDisguiser, Disguiser>(() => new Disguiser(this));
        public IFolderDeleter FolderDeleter => DefaultComponent<IFolderDeleter, FolderDeleter>();
        public IFolderUpdater FolderUpdater => DefaultComponent<IFolderUpdater, FolderUpdater>();
        public IPassphraseProvider PassphraseProvider => DefaultComponent<IPassphraseProvider, PassphraseProvider>();
        public IPeghEnvironment PeghEnvironment => DefaultComponent<IPeghEnvironment, PeghEnvironment>();
        public IPrimeNumberGenerator PrimeNumberGenerator => DefaultComponent<IPrimeNumberGenerator, PrimeNumberGenerator>();
        public ISecretRepository SecretRepository => DefaultComponent<ISecretRepository, SecretRepository>(() => new SecretRepository(this));
        public IStringCrypter StringCrypter => DefaultComponent<IStringCrypter, StringCrypter>(() => new StringCrypter(this));
        public IXmlDeserializer XmlDeserializer => DefaultComponent<IXmlDeserializer, XmlDeserializer>();
        public IXmlSerializer XmlSerializer => DefaultComponent<IXmlSerializer, XmlSerializer>();
        public IXmlSchemer XmlSchemer => DefaultComponent<IXmlSchemer, XmlSchemer>();

        public void SetAppDataSpecialFolder(IFolder folder) {
            if (DefaultComponents.ContainsKey(typeof(IPeghEnvironment))) {
                throw new Exception("Pegh environment has already been set");
            }

            DefaultComponents[typeof(IPeghEnvironment)] = new PeghEnvironment(folder);
        }
    }
}
