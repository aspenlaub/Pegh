using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities.Test {
    [TestClass]
    public class SecretStringCrypterFunctionTest {
        [TestMethod]
        public void CanEncryptAndDecryptStrings() {
            var encrypterSecret = new SecretStringEncrypterFunction();
            Assert.IsFalse(string.IsNullOrEmpty(encrypterSecret.Guid));
            var decrypterSecret = new SecretStringDecrypterFunction();
            Assert.IsFalse(string.IsNullOrEmpty(decrypterSecret.Guid));

            var componentProvider = new ComponentProvider();
            var secretRepository = componentProvider.SecretRepository;
            var errorsAndInfos = new ErrorsAndInfos();
            var realEncrypterSecret = secretRepository.Get(encrypterSecret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            const string originalString = "Whatever you do not want to reveal, keep it secret!";
            var encryptedString = secretRepository.ExecutePowershellFunction(encrypterSecret.DefaultValue, originalString);
            Assert.AreNotEqual(originalString, encryptedString);

            var realDecrypterSecret = secretRepository.Get(decrypterSecret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            var decryptedString = secretRepository.ExecutePowershellFunction(decrypterSecret.DefaultValue, encryptedString);
            Assert.AreEqual(originalString, decryptedString);

            encryptedString = secretRepository.ExecutePowershellFunction(realEncrypterSecret, originalString);
            Assert.AreNotEqual(originalString, encryptedString);
            decryptedString = secretRepository.ExecutePowershellFunction(realDecrypterSecret, encryptedString);
            Assert.AreEqual(originalString, decryptedString);
        }
    }
}
