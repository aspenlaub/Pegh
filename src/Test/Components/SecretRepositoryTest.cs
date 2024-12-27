using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class SecretRepositoryTest {
    protected IContainer Container;
    protected IContainer AlternativeContainer;
    protected IContainer ContainerWithMockedDisguiser;

    protected IFolder AppDataSpecialFolder { get; }

    protected const string SomeFirstName = "Some First Name", SomeSurName = "Some Surname", SomeRank = "Some Rank";
    protected const string Passphrase = "DbDy38Dk973-5DeC9-4A.10-A7$45-DB§66C15!!05B80";
    protected Mock<ICsArgumentPrompter> CsArgumentPrompterMock, AlternativeCsArgumentPrompterMock;

    public SecretRepositoryTest() {
        AppDataSpecialFolder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder("NoSecrets");
        if (AppDataSpecialFolder.Exists()) {
            var deleter = new FolderDeleter();
            deleter.DeleteFolder(AppDataSpecialFolder);
        }
        AppDataSpecialFolder.CreateIfNecessary();
    }

    [TestInitialize]
    public void Initialize() {
        CsArgumentPrompterMock = new Mock<ICsArgumentPrompter>();
        CsArgumentPrompterMock.Setup(p => p.PromptForArgument(It.IsAny<string>(), It.IsAny<string>())).Returns(Passphrase);

        var builder = new ContainerBuilder().UseForPeghTest(CsArgumentPrompterMock.Object);
        Container = builder.Build();

        AlternativeCsArgumentPrompterMock = new Mock<ICsArgumentPrompter>();
        AlternativeCsArgumentPrompterMock.Setup(p => p.PromptForArgument(It.IsAny<string>(), It.IsAny<string>())).Returns(Passphrase);

        var peghEnvironment = new PeghEnvironment(AppDataSpecialFolder);
        builder = new ContainerBuilder().UseForPeghTest(peghEnvironment, AlternativeCsArgumentPrompterMock.Object);
        AlternativeContainer = builder.Build();

        var disguiserMock = new Mock<IDisguiser>();
        disguiserMock.Setup(d => d.Disguise(It.IsAny<ISecretRepository>(), It.IsAny<string>(), It.IsAny<IErrorsAndInfos>())).Returns(Task.FromResult(""));
        builder = new ContainerBuilder().UseForPeghTest(disguiserMock.Object, CsArgumentPrompterMock.Object);
        ContainerWithMockedDisguiser = builder.Build();

        builder = new ContainerBuilder().UseForPeghTest(disguiserMock.Object, AlternativeCsArgumentPrompterMock.Object);
        builder.Build();

        SecretRepositoryFolder(false, false);
        SecretRepositoryFolder(true, false);
        SecretRepositoryFolder(false, true);
        SecretRepositoryFolder(true, true);
    }

    [ClassCleanup]
    public static void ClassCleanup() {
        var folder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder("NoSecrets");
        if (!folder.Exists()) { return; }


        var deleter = new FolderDeleter();
        deleter.DeleteFolder(folder);
    }

    private string SecretRepositoryFolder(bool sample, bool alternative) {
        var container = alternative ? AlternativeContainer : Container;
        var folder = container.Resolve<IPeghEnvironment>().RootWorkFolder + (sample ? @"\SecretSamples" : @"\SecretRepository");
        if (!Directory.Exists(folder)) {
            Directory.CreateDirectory(folder);
        }
        return folder;
    }

    protected static CrewMember GetSecretCrewMember(SecretRepository sut, IGuid secret) {
        return sut.Values.TryGetValue(secret.Guid, out object crewMember) ? crewMember as CrewMember : null;
    }

    protected static ListOfElements GetSecretListOfElements(SecretRepository sut, IGuid secret) {
        return sut.Values.TryGetValue(secret.Guid, out object secretList) ? secretList as ListOfElements : null;
    }

    [TestMethod]
    public void DoesNotExistInRepositoryAfterRemoval() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var secret = new SecretCrewMember();
        sut.Reset(secret);
        Assert.IsFalse(sut.Exists(secret));
    }

    [TestMethod]
    public async Task CanGetDefault() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, true, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        var secret = new SecretCrewMember();
        sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName };
        sut.Reset(secret);
        Assert.IsNull(GetSecretCrewMember(sut, secret));
        var crewMember = await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        Assert.AreEqual(SecretCrewMember.DefaultFirstName, crewMember.FirstName);
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task ExistsAfterSetting() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var secret = new SecretCrewMember();
        sut.Reset(secret);
        var errorsAndInfos = new ErrorsAndInfos();
        await sut.SetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        Assert.IsTrue(sut.Exists(secret));
        sut.Reset(secret);
        Assert.IsFalse(sut.Exists(secret));
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task CanGetAfterSetting() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var secret = new SecretCrewMember();
        sut.Reset(secret);
        sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
        var errorsAndInfos = new ErrorsAndInfos();
        await sut.SetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        sut.Values.Remove(secret.Guid);
        Assert.IsNull(GetSecretCrewMember(sut, secret));
        await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        Assert.AreEqual(SomeFirstName, GetSecretCrewMember(sut, secret).FirstName);
        Assert.AreEqual(SomeSurName, GetSecretCrewMember(sut, secret).SurName);
        Assert.AreEqual(SomeRank, GetSecretCrewMember(sut, secret).Rank);
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task CanGetScriptSecret() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, true, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        var secret = new SecretStringFunction();
        sut.Reset(secret);
        const string s = "This is not a string";
        var r = (await sut.CompileCsLambdaAsync<string, string>(await sut.GetAsync(secret, errorsAndInfos)))(s);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        Assert.IsTrue(r.StartsWith(s));
        Assert.IsTrue(r.Contains("with greetings from a csx"));
    }

    private void CleanUpSecretRepository(bool alternative) {
        var secrets = new List<IGuid> { new SecretCrewMember(), new SecretStringFunction(), new SecretListOfElements() };
        foreach (var files in new[] { false, true }.Select(sample => SecretRepositoryFolder(sample, alternative)).SelectMany(folder => secrets.Select(secret => Directory.GetFiles(folder, secret.Guid + "*.*").ToList()))) {
            Assert.IsTrue(files.Count < 3);
            files.ForEach(File.Delete);
        }
    }

    [TestMethod]
    public void DefaultSecretsAreStoredByDefault() {
        var secret = new SecretShouldDefaultSecretsBeStored();
        var shouldDefaultSecretsBeStored = secret.DefaultValue;
        Assert.IsNotNull(shouldDefaultSecretsBeStored);
        Assert.IsTrue(shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent);
    }

    [TestMethod]
    public async Task DefaultSecretIsSavedIfSecretSaysSo() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, true, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        var secret = new SecretCrewMember();
        sut.Reset(secret);
        await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        Assert.IsTrue(sut.Exists(secret));
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task DefaultSecretIsNotSavedIfSecretSaysNo() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, false, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        var secret = new SecretCrewMember();
        sut.Reset(secret);
        await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase)), errorsAndInfos.ErrorsToString());
        Assert.IsFalse(sut.Exists(secret));
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task DefaultSecretIsNotReturnedIfSecretSaysItShouldNotBeSaved() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, false, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        var secret = new SecretCrewMember();
        sut.Reset(secret);
        Assert.IsNull(await sut.GetAsync(secret, errorsAndInfos));
        Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase)), errorsAndInfos.ErrorsToString());
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task DefaultSecretIsNotCachedIfSecretSaysItShouldNotBeSaved() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, false, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        var secret = new SecretCrewMember();
        sut.Reset(secret);
        await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase)), errorsAndInfos.ErrorsToString());
        Assert.IsFalse(sut.Values.ContainsKey(secret.Guid));
        CleanUpSecretRepository(false);
    }

    private static async Task SetShouldDefaultSecretsBeStored(ISecretRepository sut, bool shouldThey, IErrorsAndInfos errorsAndInfos) {
        var shouldDefaultSecretsBeStored = await ShouldDefaultSecretsBeStoredAsync(sut, errorsAndInfos);
        if (shouldThey == shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent) {
            return;
        }

        shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent = shouldThey;
        shouldDefaultSecretsBeStored = await ShouldDefaultSecretsBeStoredAsync(sut, errorsAndInfos);
        Assert.AreEqual(shouldThey, shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent);
    }

    private static async Task<ShouldDefaultSecretsBeStored> ShouldDefaultSecretsBeStoredAsync(ISecretRepository sut, IErrorsAndInfos errorsAndInfos) {
        var secret = new SecretShouldDefaultSecretsBeStored();
        var shouldDefaultSecretsBeStored = await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsNotNull(shouldDefaultSecretsBeStored);
        return shouldDefaultSecretsBeStored;
    }

    [TestMethod]
    public async Task DefaultScriptSecretIsSavedIfSecretSaysSo() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, true, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        var secret = new SecretStringFunction();
        sut.Reset(secret);
        await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        Assert.IsTrue(sut.Exists(secret));
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task DefaultScriptSecretIsNotSavedIfSecretSaysNo() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, false, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        var secret = new SecretStringFunction();
        sut.Reset(secret);
        await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase)), errorsAndInfos.ErrorsToString());
        Assert.IsFalse(sut.Exists(secret));
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task DefaultScriptSecretIsNotReturnedIfSecretSaysItShouldNotBeSaved() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, false, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        var secret = new SecretStringFunction();
        sut.Reset(secret);
        Assert.IsNull(await sut.GetAsync(secret, errorsAndInfos));
        Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase)), errorsAndInfos.ErrorsToString());
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task DefaultScriptSecretIsNotCachedIfSecretSaysItShouldNotBeSaved() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, false, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        var secret = new SecretStringFunction();
        sut.Reset(secret);
        await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase)), errorsAndInfos.ErrorsToString());
        Assert.IsFalse(sut.Values.ContainsKey(secret.Guid));
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task SavedScriptSecretIsUsedDuringExecution() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, true, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        var secret = new SecretStringFunction();
        sut.Reset(secret);
        var script = await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        const string addedString = "/* This script has been altered */";
        Assert.IsFalse(script.LambdaExpression.Contains(addedString));
        script.LambdaExpression = addedString + "\r\n" + script.LambdaExpression;
        await sut.SetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        sut.Values.Clear();
        await sut.ValueOrDefaultAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        script = (CsLambda)sut.Values[secret.Guid];
        Assert.IsTrue(script.LambdaExpression.StartsWith(addedString));
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task DefaultSecretSampleIsSavedIfSecretSaysSo() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, true, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        CleanUpSecretRepository(false);

        var secret = new SecretCrewMember();
        var folder = SecretRepositoryFolder(true, false);
        Assert.AreEqual(0, Directory.GetFiles(folder, secret.Guid + "*.*").Length);
        sut.SaveSample(secret, false);
        Assert.AreEqual(1, Directory.GetFiles(folder, secret.Guid + "*.xml").Length);
        Assert.AreEqual(1, Directory.GetFiles(folder, secret.Guid + "*.xsd").Length);
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task DefaultSecretSampleIsSavedEvenIfSecretSaysNo() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, false, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        CleanUpSecretRepository(false);

        var secret = new SecretCrewMember();
        var folder = SecretRepositoryFolder(true, false);
        Assert.AreEqual(0, Directory.GetFiles(folder, secret.Guid + "*.*").Length);
        sut.SaveSample(secret, false);
        Assert.AreEqual(1, Directory.GetFiles(folder, secret.Guid + "*.xml").Length);
        Assert.AreEqual(1, Directory.GetFiles(folder, secret.Guid + "*.xsd").Length);
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task CanGetLongSecretString() {
        var sut = ContainerWithMockedDisguiser.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var errorsAndInfos = new ErrorsAndInfos();
        await SetShouldDefaultSecretsBeStored(sut, true, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        var secret = new LongSecretString();
        var longSecretString = await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        Assert.AreEqual(128, longSecretString.TheLongString.Length);

        secret = new LongSecretString();
        Assert.AreEqual(128, secret.DefaultValue.TheLongString.Length);
        Assert.AreNotEqual(longSecretString.TheLongString, secret.DefaultValue.TheLongString);
    }

    [TestMethod]
    public async Task DoesNotExistAfterTryingToSaveInvalidXml() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var secret = new SecretCrewMember();
        sut.Reset(secret);
        var valueOrDefault = secret.DefaultValue;
        var xml = Container.Resolve<IXmlSerializer>().Serialize(valueOrDefault).Replace("Crew", "Curfew");
        var errorsAndInfos = new ErrorsAndInfos();
        await sut.WriteToFileAsync(secret, xml, false, errorsAndInfos);
        Assert.IsTrue(errorsAndInfos.Errors.All(e => e.Contains("The \'http://www.aspenlaub.net:CurfewMember\' element is not declared")), errorsAndInfos.ErrorsToString());
        Assert.IsFalse(sut.Exists(secret));
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task CanSetAndGetSecretListOfElements() {
        var sut = ContainerWithMockedDisguiser.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);

        var secret = new SecretListOfElements();
        sut.Reset(secret);
        var listOfElements = new ListOfElements { new() { Value = "One" }, new() { Value = "Two" }};
        sut.Values[secret.Guid] = listOfElements;
        var errorsAndInfos = new ErrorsAndInfos();
        await sut.SetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        CsArgumentPrompterMock.Setup(p => p.PromptForArgument(It.IsAny<string>(), It.IsAny<string>())).Returns(Passphrase + Passphrase);
        await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        listOfElements = GetSecretListOfElements(sut, secret);
        Assert.AreEqual(2, listOfElements.Count);
        Assert.AreEqual("Two", listOfElements[1].Value);
        CleanUpSecretRepository(false);
    }

    [TestMethod]
    public async Task CanWorkWithAlternativePeghEnvironment() {
        var sut = Container.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(sut);
        var alternativeSut = AlternativeContainer.Resolve<ISecretRepository>() as SecretRepository;
        Assert.IsNotNull(alternativeSut);

        var secret = new SecretCrewMember();
        sut.Reset(secret);
        sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
        var errorsAndInfos = new ErrorsAndInfos();
        await sut.SetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        secret = new SecretCrewMember();
        alternativeSut.Reset(secret);
        alternativeSut.Values[secret.Guid] = new CrewMember { FirstName = "ALT " + SomeFirstName, SurName = "ALT" + SomeSurName, Rank = "ALT" + SomeRank };
        errorsAndInfos = new ErrorsAndInfos();
        await alternativeSut.SetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

        sut.Values.Remove(secret.Guid);
        alternativeSut.Values.Remove(secret.Guid);

        await alternativeSut.GetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        Assert.AreEqual("ALT " + SomeFirstName, GetSecretCrewMember(alternativeSut, secret).FirstName);

        await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        Assert.AreEqual(SomeFirstName, GetSecretCrewMember(sut, secret).FirstName);

        CleanUpSecretRepository(false);
        CleanUpSecretRepository(true);
    }
}