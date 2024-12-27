using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class PassphraseProviderTest {
    private const string _expectedPassphrase = "This is not a passphrase";
    private const string _passphraseGuid = "0DBFD7FC-B717-4212-BB85-44E25C7D0EE4";
    private const string _title = "This is not a title";
    private const string _description = "This is not a description";

    [TestInitialize]
    public void Initialize() {
        PassphraseProvider.Passphrases.Clear();
    }

    [TestMethod]
    public void CanProvidePassphrase() {
        IPassphraseProvider sut = new PassphraseProvider();
        var actualPassphrase = sut.Passphrase(_passphraseGuid, _title, _description, SuccessfulPassphraseDialog);
        Assert.AreEqual(_expectedPassphrase, actualPassphrase);
    }

    [TestMethod]
    public void PassphraseIsEmptyIfDialogFails() {
        IPassphraseProvider sut = new PassphraseProvider();
        var actualPassphrase = sut.Passphrase(_passphraseGuid, _title, _description, FailedPassphraseDialog);
        Assert.AreEqual("", actualPassphrase);
    }

    [TestMethod]
    public void CachedPassphraseIsReturned() {
        IPassphraseProvider sut = new PassphraseProvider();
        var actualPassphrase = sut.Passphrase(_passphraseGuid, _title, _description, SuccessfulPassphraseDialog);
        Assert.AreEqual(_expectedPassphrase, actualPassphrase);
        actualPassphrase = sut.Passphrase(_passphraseGuid, _title, _description, FailedPassphraseDialog);
        Assert.AreEqual(_expectedPassphrase, actualPassphrase);
    }

    [TestMethod]
    public void NoPassphraseAfterCacheIsCleared() {
        IPassphraseProvider sut = new PassphraseProvider();
        var actualPassphrase = sut.Passphrase(_passphraseGuid, _title, _description, SuccessfulPassphraseDialog);
        Assert.AreEqual(_expectedPassphrase, actualPassphrase);
        PassphraseProvider.Passphrases.Clear();
        actualPassphrase = sut.Passphrase(_passphraseGuid, _title, _description, FailedPassphraseDialog);
        Assert.AreEqual("", actualPassphrase);
    }

    [TestMethod]
    public void NoPassphraseIfDialogIsNotAvailable() {
        IPassphraseProvider sut = new PassphraseProvider();
        var actualPassphrase = sut.Passphrase(_passphraseGuid, _title, _description, () => null);
        Assert.AreEqual("", actualPassphrase);
    }

    private static IPassphraseDialog SuccessfulPassphraseDialog() {
        var passphraseDialogMock = new Mock<IPassphraseDialog>();
        passphraseDialogMock.Setup(p => p.ShowDialog()).Returns(true);
        passphraseDialogMock.Setup(p => p.Passphrase()).Returns(_expectedPassphrase);
        return passphraseDialogMock.Object;
    }

    private static IPassphraseDialog FailedPassphraseDialog() {
        var passphraseDialogMock = new Mock<IPassphraseDialog>();
        passphraseDialogMock.Setup(p => p.ShowDialog()).Returns(false);
        return passphraseDialogMock.Object;
    }
}