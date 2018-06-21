﻿using System;
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

        public IDisguiser Disguiser { get { return DefaultComponent<IDisguiser, Disguiser>(() => new Disguiser(this)); } }
        public IFolderDeleter FolderDeleter { get { return DefaultComponent<IFolderDeleter, FolderDeleter>(); } }
        public IFolderUpdater FolderUpdater { get { return DefaultComponent<IFolderUpdater, FolderUpdater>(); } }
        public IPassphraseProvider PassphraseProvider { get { return DefaultComponent<IPassphraseProvider, PassphraseProvider>(); } }
        public IPeghEnvironment PeghEnvironment { get { return DefaultComponent<IPeghEnvironment, PeghEnvironment>(); } }
        public IPowershellExecuter PowershellExecuter { get { return DefaultComponent<IPowershellExecuter, PowershellExecuter>(); } }
        public IPrimeNumberGenerator PrimeNumberGenerator { get { return DefaultComponent<IPrimeNumberGenerator, PrimeNumberGenerator>(); } }
        public ISecretRepository SecretRepository { get { return DefaultComponent<ISecretRepository, SecretRepository>(() => new SecretRepository(this)); } }
        public IXmlDeserializer XmlDeserializer { get { return DefaultComponent<IXmlDeserializer, XmlDeserializer>(); } }
        public IXmlSerializer XmlSerializer { get { return DefaultComponent<IXmlSerializer, XmlSerializer>(); } }
        public IXmlSchemer XmlSchemer { get { return DefaultComponent<IXmlSchemer, XmlSchemer>(); } }

        public void SetAppDataSpecialFolder(IFolder folder) {
            if (DefaultComponents.ContainsKey(typeof(IPeghEnvironment))) {
                throw new Exception("Pegh environment has already been set");
            }

            DefaultComponents[typeof(IPeghEnvironment)] = new PeghEnvironment(folder);
        }
    }
}
