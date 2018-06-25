using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class StringCrypter : IStringCrypter {
        protected IComponentProvider ComponentProvider;
        protected PowershellFunction<string, string> SecretEncrypter;
        protected PowershellFunction<string, string> SecretDecrypter;

        public StringCrypter(IComponentProvider componentProvider) {
            ComponentProvider = componentProvider;

            var encrypterSecret = new SecretStringEncrypterFunction();
            var decrypterSecret = new SecretStringDecrypterFunction();
            var errorsAndInfos = new ErrorsAndInfos();
            SecretEncrypter = ComponentProvider.SecretRepository.Get(encrypterSecret, errorsAndInfos);
            SecretDecrypter = ComponentProvider.SecretRepository.Get(decrypterSecret, errorsAndInfos);
            if (!errorsAndInfos.AnyErrors()) { return; }

            throw new Exception(string.Join("\r\n", errorsAndInfos.Errors));
        }

        public string Encrypt(string s) {
            return ComponentProvider.SecretRepository.ExecutePowershellFunction(SecretEncrypter, s);
        }

        public string Decrypt(string s) {
            return ComponentProvider.SecretRepository.ExecutePowershellFunction(SecretDecrypter, s);
        }
    }
}