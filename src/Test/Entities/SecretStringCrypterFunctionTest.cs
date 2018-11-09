using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities {
    [TestClass]
    public class SecretStringCrypterFunctionTest {
        [TestMethod]
        public void CanEncryptAndDecryptStrings() {
            var componentProvider = new ComponentProvider();
            var secretRepository = componentProvider.SecretRepository;

            var encrypterSecret = new SecretStringEncrypterFunction();
            Assert.IsFalse(string.IsNullOrEmpty(encrypterSecret.Guid));
            var secretEncrypterFunction = secretRepository.CompileCsLambdaAsync<string, string>(encrypterSecret.DefaultValue).Result;

            var decrypterSecret = new SecretStringDecrypterFunction();
            Assert.IsFalse(string.IsNullOrEmpty(decrypterSecret.Guid));
            var secretDecrypterFunction = secretRepository.CompileCsLambdaAsync<string, string>(decrypterSecret.DefaultValue).Result;

            const string originalString = "Whatever you do not want to reveal, keep it secret (\\, € ✂ and ❤)!";
            var encryptedString = secretEncrypterFunction(originalString);
            Assert.AreNotEqual(originalString, encryptedString);
            var decryptedString = secretDecrypterFunction(encryptedString);
            Assert.AreEqual(originalString, decryptedString);
        }

        [TestMethod]
        public void CanEncryptAndDecryptStringsUsingRealEncrypter() {
            var componentProvider = new ComponentProvider();
            var secretRepository = componentProvider.SecretRepository;

            var encrypterSecret = new SecretStringEncrypterFunction();
            Assert.IsFalse(string.IsNullOrEmpty(encrypterSecret.Guid));
            var errorsAndInfos = new ErrorsAndInfos();
            var csLambda = secretRepository.GetAsync(encrypterSecret, errorsAndInfos).Result;
            Assert.IsFalse(errorsAndInfos.AnyErrors(), string.Join("\r\n", errorsAndInfos.Errors));
            var secretEncrypterFunction = secretRepository.CompileCsLambdaAsync<string, string>(csLambda).Result;

            var decrypterSecret = new SecretStringDecrypterFunction();
            Assert.IsFalse(string.IsNullOrEmpty(decrypterSecret.Guid));
            csLambda = secretRepository.GetAsync(decrypterSecret, errorsAndInfos).Result;
            Assert.IsFalse(errorsAndInfos.AnyErrors(), string.Join("\r\n", errorsAndInfos.Errors));
            var secretDecrypterFunction = secretRepository.CompileCsLambdaAsync<string, string>(csLambda).Result;

            const string originalString = "Whatever you do not want to reveal, keep it secret (\\, € ✂ and ❤)!";

            var encryptedString = secretEncrypterFunction(originalString);
            Assert.AreNotEqual(originalString, encryptedString);

            var decryptedString = secretDecrypterFunction(encryptedString);
            Assert.AreEqual(originalString, decryptedString);
        }
    }
}
