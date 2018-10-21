using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities {
    [TestClass]
    public class SecretPassphraseFunctionTest {
        [TestMethod]
        public void SecretPassphraseIsDefaultIfNoUserIsPresent() {
            var secret = new SecretPassphraseFunction();
            Assert.IsFalse(string.IsNullOrEmpty(secret.Guid));
            var componentProvider = new ComponentProvider();
            var secretRepository = componentProvider.SecretRepository;
            var errorsAndInfos = new ErrorsAndInfos();
            secretRepository.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            const string defaultPassPhrase = "This is not a pass phrase";
            var arg = new SecretPassphraseFunctionArgument { IsUserPresent = false, PassphraseIfUserIsNotPresent = defaultPassPhrase };
            var passPhrase = secretRepository.ExecutePowershellFunction(secret.DefaultValue, arg);
            Assert.AreEqual(defaultPassPhrase, passPhrase);
        }
    }
}
