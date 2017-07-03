using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class PassphraseProviderTest {
        [TestMethod]
        public void CanProvidePassphrase() {
            var sut = new PassphraseProvider();
            const string expectedPassphrase = "This is not a passphrase";
            var passphraseDialogMock = new Mock<IPassphraseDialog>();
            passphraseDialogMock.Setup(p => p.ShowDialog()).Returns(true);
            passphraseDialogMock.Setup(p => p.Passphrase()).Returns(expectedPassphrase);
            var actualPassphrase = sut.Passphrase(passphraseDialogMock.Object);
            Assert.AreEqual(expectedPassphrase, actualPassphrase);
            passphraseDialogMock.Setup(p => p.ShowDialog()).Returns(false);
            actualPassphrase = sut.Passphrase(passphraseDialogMock.Object);
            Assert.AreEqual("", actualPassphrase);
        }
    }
}
