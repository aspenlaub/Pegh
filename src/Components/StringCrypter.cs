using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class StringCrypter : IStringCrypter {
        protected IComponentProvider ComponentProvider;
        protected Func<string, string> SecretEncrypterFunction;
        protected Func<string, string> SecretDecrypterFunction;

        public StringCrypter(IComponentProvider componentProvider) {
            ComponentProvider = componentProvider;

            var encrypterSecret = new SecretStringEncrypterFunction();
            var errorsAndInfos = new ErrorsAndInfos();
            var csLambda = ComponentProvider.SecretRepository.GetAsync(encrypterSecret, errorsAndInfos).Result;
            SecretEncrypterFunction = ComponentProvider.SecretRepository.CompileCsLambdaAsync<string, string>(csLambda).Result;

            var decrypterSecret = new SecretStringDecrypterFunction();
            csLambda = ComponentProvider.SecretRepository.GetAsync(decrypterSecret, errorsAndInfos).Result;
            SecretDecrypterFunction = ComponentProvider.SecretRepository.CompileCsLambdaAsync<string, string>(csLambda).Result;

            if (!errorsAndInfos.AnyErrors()) { return; }

            throw new Exception(string.Join("\r\n", errorsAndInfos.Errors));
        }

        public string Encrypt(string s) {
            return SecretEncrypterFunction(s);
        }

        public string Decrypt(string s) {
            return SecretDecrypterFunction(s);
        }
    }
}