using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class StringCrypter : IStringCrypter {
        protected Func<string, string> SecretEncrypterFunction;
        protected Func<string, string> SecretDecrypterFunction;

        protected readonly ISecretRepository SecretRepository;

        public StringCrypter(ISecretRepository secretRepository) {
            SecretRepository = secretRepository;

            var encrypterSecret = new SecretStringEncrypterFunction();
            var errorsAndInfos = new ErrorsAndInfos();
            var csLambda = SecretRepository.GetAsync(encrypterSecret, errorsAndInfos).Result;
            SecretEncrypterFunction = SecretRepository.CompileCsLambdaAsync<string, string>(csLambda).Result;

            var decrypterSecret = new SecretStringDecrypterFunction();
            csLambda = SecretRepository.GetAsync(decrypterSecret, errorsAndInfos).Result;
            SecretDecrypterFunction = SecretRepository.CompileCsLambdaAsync<string, string>(csLambda).Result;

            if (!errorsAndInfos.AnyErrors()) { return; }

            throw new Exception(errorsAndInfos.ErrorsToString());
        }

        public string Encrypt(string s) {
            return SecretEncrypterFunction(s);
        }

        public string Decrypt(string s) {
            return SecretDecrypterFunction(s);
        }
    }
}