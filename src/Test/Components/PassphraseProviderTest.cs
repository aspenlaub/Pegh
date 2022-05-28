using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class PassphraseProviderTest {
    private const string ExpectedPassphrase = "This is not a passphrase";
    private const string PassphraseGuid = "0DBFD7FC-B717-4212-BB85-44E25C7D0EE4";
    private const string Title = "This is not a title";
    private const string Description = "This is not a description";

    [TestInitialize]
    public void Initialize() {
        PassphraseProvider.Passphrases.Clear();
    }

    [TestMethod]
    public void CanProvidePassphrase() {
        IPassphraseProvider sut = new PassphraseProvider();
        var actualPassphrase = sut.Passphrase(PassphraseGuid, Title, Description, SuccessfulPassphraseDialog);
        Assert.AreEqual(ExpectedPassphrase, actualPassphrase);
    }

    [TestMethod]
    public void PassphraseIsEmptyIfDialogFails() {
        IPassphraseProvider sut = new PassphraseProvider();
        var actualPassphrase = sut.Passphrase(PassphraseGuid, Title, Description, FailedPassphraseDialog);
        Assert.AreEqual("", actualPassphrase);
    }

    [TestMethod]
    public void CachedPassphraseIsReturned() {
        IPassphraseProvider sut = new PassphraseProvider();
        var actualPassphrase = sut.Passphrase(PassphraseGuid, Title, Description, SuccessfulPassphraseDialog);
        Assert.AreEqual(ExpectedPassphrase, actualPassphrase);
        actualPassphrase = sut.Passphrase(PassphraseGuid, Title, Description, FailedPassphraseDialog);
        Assert.AreEqual(ExpectedPassphrase, actualPassphrase);
    }

    [TestMethod]
    public void NoPassphraseAfterCacheIsCleared() {
        IPassphraseProvider sut = new PassphraseProvider();
        var actualPassphrase = sut.Passphrase(PassphraseGuid, Title, Description, SuccessfulPassphraseDialog);
        Assert.AreEqual(ExpectedPassphrase, actualPassphrase);
        PassphraseProvider.Passphrases.Clear();
        actualPassphrase = sut.Passphrase(PassphraseGuid, Title, Description, FailedPassphraseDialog);
        Assert.AreEqual("", actualPassphrase);
    }

    [TestMethod]
    public void NoPassphraseIfDialogIsNotAvailable() {
        IPassphraseProvider sut = new PassphraseProvider();
        var actualPassphrase = sut.Passphrase(PassphraseGuid, Title, Description, () => null);
        Assert.AreEqual("", actualPassphrase);
    }

    private static IPassphraseDialog SuccessfulPassphraseDialog() {
        var passphraseDialogMock = new Mock<IPassphraseDialog>();
        passphraseDialogMock.Setup(p => p.ShowDialog()).Returns(true);
        passphraseDialogMock.Setup(p => p.Passphrase()).Returns(ExpectedPassphrase);
        return passphraseDialogMock.Object;
    }

    private static IPassphraseDialog FailedPassphraseDialog() {
        var passphraseDialogMock = new Mock<IPassphraseDialog>();
        passphraseDialogMock.Setup(p => p.ShowDialog()).Returns(false);
        return passphraseDialogMock.Object;
    }
}