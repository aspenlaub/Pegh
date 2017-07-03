using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities.Test {
    [TestClass]
    public class SecretPassphraseFunctionTest {
        [TestMethod]
        public void SecretPassphraseIsDefaultIfNoUserIsPresent() {
            var secret = new SecretPassphraseFunction();
            Assert.IsFalse(string.IsNullOrEmpty(secret.Guid));
            var componentProvider = new ComponentProvider();
            var secretRepository = componentProvider.SecretRepository;
            secretRepository.Get(secret);
            const string defaultPassPhrase = "This is not a pass phrase";
            var arg = new SecretPassphraseFunctionArgument { IsUserPresent = false, PassphraseIfUserIsNotPresent = defaultPassPhrase };
            var passPhrase = secretRepository.ExecutePowershellFunction(secret.DefaultValue, arg);
            Assert.AreEqual(defaultPassPhrase, passPhrase);
        }
    }
}
