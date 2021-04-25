using System;
using System.Threading.Tasks;
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
        }

        protected async Task LoadSecretsIfNecessaryAsync() {
            if (SecretEncrypterFunction != null) { return; }

            var encrypterSecret = new SecretStringEncrypterFunction();
            var errorsAndInfos = new ErrorsAndInfos();
            var csLambda = await SecretRepository.GetAsync(encrypterSecret, errorsAndInfos);
            SecretEncrypterFunction = await SecretRepository.CompileCsLambdaAsync<string, string>(csLambda);

            var decrypterSecret = new SecretStringDecrypterFunction();
            csLambda = await SecretRepository.GetAsync(decrypterSecret, errorsAndInfos);
            SecretDecrypterFunction = await SecretRepository.CompileCsLambdaAsync<string, string>(csLambda);

            if (!errorsAndInfos.AnyErrors()) { return; }

            throw new Exception(errorsAndInfos.ErrorsToString());
        }

        public async Task<string> EncryptAsync(string s) {
            await LoadSecretsIfNecessaryAsync();
            return SecretEncrypterFunction(s);
        }

        public async Task<string> DecryptAsync(string s) {
            await LoadSecretsIfNecessaryAsync();
            return SecretDecrypterFunction(s);
        }
    }
}