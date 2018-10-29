using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities {
    [TestClass]
    public class SecretStringCrypterFunctionTest {
        [TestMethod]
        public async Task CanEncryptAndDecryptStrings() {
            var encrypterSecret = new SecretStringEncrypterFunction();
            Assert.IsFalse(string.IsNullOrEmpty(encrypterSecret.Guid));
            var decrypterSecret = new SecretStringDecrypterFunction();
            Assert.IsFalse(string.IsNullOrEmpty(decrypterSecret.Guid));

            var componentProvider = new ComponentProvider();
            var secretRepository = componentProvider.SecretRepository;

            const string originalString = "Whatever you do not want to reveal, keep it secret!";
            var encryptedString = await secretRepository.ExecuteCsScriptAsync(encrypterSecret.DefaultValue, new List<ICsScriptArgument> { new CsScriptArgument { Name = "s", Value = originalString } });
            Assert.AreNotEqual(originalString, encryptedString);
            var decryptedString = await secretRepository.ExecuteCsScriptAsync(decrypterSecret.DefaultValue, new List<ICsScriptArgument> { new CsScriptArgument { Name = "s", Value = encryptedString } });
            Assert.AreEqual(originalString, decryptedString);
        }

        [TestMethod]
        public async Task CanEncryptAndDecryptStringsUsingRealEncrypter() {
            var encrypterSecret = new SecretStringEncrypterFunction();
            Assert.IsFalse(string.IsNullOrEmpty(encrypterSecret.Guid));
            var decrypterSecret = new SecretStringDecrypterFunction();
            Assert.IsFalse(string.IsNullOrEmpty(decrypterSecret.Guid));

            var componentProvider = new ComponentProvider();
            var secretRepository = componentProvider.SecretRepository;

            const string originalString = "Whatever you do not want to reveal, keep it secret!";

            var errorsAndInfos = new ErrorsAndInfos();
            var realEncrypterSecret = await secretRepository.GetAsync(encrypterSecret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            var realDecrypterSecret = await secretRepository.GetAsync(decrypterSecret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var encryptedString = await secretRepository.ExecuteCsScriptAsync(realEncrypterSecret, new List<ICsScriptArgument> { new CsScriptArgument { Name = "s", Value = originalString } });
            Assert.AreNotEqual(originalString, encryptedString);
            var decryptedString = await secretRepository.ExecuteCsScriptAsync(realDecrypterSecret, new List<ICsScriptArgument> { new CsScriptArgument { Name = "s", Value = encryptedString } });
            Assert.AreEqual(originalString, decryptedString);
        }
    }
}
