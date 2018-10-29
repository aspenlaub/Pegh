using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class StringCrypter : IStringCrypter {
        protected IComponentProvider ComponentProvider;
        protected ICsScript SecretEncrypter;
        protected ICsScript SecretDecrypter;

        public StringCrypter(IComponentProvider componentProvider) {
            ComponentProvider = componentProvider;

            var encrypterSecret = new SecretStringEncrypterFunction();
            var decrypterSecret = new SecretStringDecrypterFunction();
            var errorsAndInfos = new ErrorsAndInfos();
            SecretEncrypter = ComponentProvider.SecretRepository.GetAsync(encrypterSecret, errorsAndInfos).Result;
            SecretDecrypter = ComponentProvider.SecretRepository.GetAsync(decrypterSecret, errorsAndInfos).Result;
            if (!errorsAndInfos.AnyErrors()) { return; }

            throw new Exception(string.Join("\r\n", errorsAndInfos.Errors));
        }

        public async Task<string> Encrypt(string s) {
            return await ComponentProvider.SecretRepository.ExecuteCsScriptAsync(SecretEncrypter, new List<ICsScriptArgument> { new CsScriptArgument { Name =  "s", Value = s } });
        }

        public async Task<string> Decrypt(string s) {
            return await ComponentProvider.SecretRepository.ExecuteCsScriptAsync(SecretDecrypter, new List<ICsScriptArgument> { new CsScriptArgument { Name = "s", Value = s } });
        }
    }
}