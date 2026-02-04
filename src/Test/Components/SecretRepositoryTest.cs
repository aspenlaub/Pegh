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

        ContainerBuilder builder = new ContainerBuilder().UseForPeghTest();
        Container = builder.Build();

        var peghEnvironment = new PeghEnvironment(AppDataSpecialFolder);
        builder = new ContainerBuilder().UseForPeghTest(peghEnvironment);
        AlternativeContainer = builder.Build();

        var disguiserMock = new Mock<IDisguiser>();
        disguiserMock.Setup(d => d.Disguise(It.IsAny<ISecretRepository>(), It.IsAny<string>(), It.IsAny<IErrorsAndInfos>())).Returns(Task.FromResult(""));
        builder = new ContainerBuilder().UseForPeghTest(disguiserMock.Object);
        ContainerWithMockedDisguiser = builder.Build();

        builder = new ContainerBuilder().UseForPeghTest(disguiserMock.Object);
        builder.Build();

        SecretRepositoryFolder(false, false);
        SecretRepositoryFolder(true, false);
        SecretRepositoryFolder(false, true);
        SecretRepositoryFolder(true, true);
    }

    [ClassCleanup]
    public static void ClassCleanup() {
        IFolder folder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder("NoSecrets");
        if (!folder.Exists()) { return; }


        var deleter = new FolderDeleter();
        deleter.DeleteFolder(folder);
    }

    private string SecretRepositoryFolder(bool sample, bool alternative) {
        IContainer container = alternative ? AlternativeContainer : Container;
        string folder = container.Resolve<IPeghEnvironment>().RootWorkFolder + (sample ? @"\SecretSamples" : @"\SecretRepository");
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
        CrewMember crewMember = await sut.GetAsync(secret, errorsAndInfos);
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
        string r = (await sut.CompileCsLambdaAsync<string, string>(await sut.GetAsync(secret, errorsAndInfos)))(s);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        Assert.StartsWith(s, r);
        Assert.Contains("with greetings from a csx", r);
    }

    private void CleanUpSecretRepository(bool alternative) {
        var secrets = new List<IGuid> { new SecretCrewMember(), new SecretStringFunction(), new SecretListOfElements() };
        foreach (List<string> files in new[] { false, true }.Select(sample => SecretRepositoryFolder(sample, alternative)).SelectMany(folder => secrets.Select(secret => Directory.GetFiles(folder, secret.Guid + "*.*").ToList()))) {
            Assert.IsLessThan(3, files.Count);
            files.ForEach(File.Delete);
        }
    }

    [TestMethod]
    public void DefaultSecretsAreStoredByDefault() {
        var secret = new SecretShouldDefaultSecretsBeStored();
        ShouldDefaultSecretsBeStored shouldDefaultSecretsBeStored = secret.DefaultValue;
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
        Assert.Contains(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase), errorsAndInfos.Errors, errorsAndInfos.ErrorsToString());
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
        Assert.Contains(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase), errorsAndInfos.Errors, errorsAndInfos.ErrorsToString());
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
        Assert.Contains(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase), errorsAndInfos.Errors, errorsAndInfos.ErrorsToString());
        Assert.IsFalse(sut.Values.ContainsKey(secret.Guid));
        CleanUpSecretRepository(false);
    }

    private static async Task SetShouldDefaultSecretsBeStored(ISecretRepository sut, bool shouldThey, IErrorsAndInfos errorsAndInfos) {
        ShouldDefaultSecretsBeStored shouldDefaultSecretsBeStored = await ShouldDefaultSecretsBeStoredAsync(sut, errorsAndInfos);
        if (shouldThey == shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent) {
            return;
        }

        shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent = shouldThey;
        shouldDefaultSecretsBeStored = await ShouldDefaultSecretsBeStoredAsync(sut, errorsAndInfos);
        Assert.AreEqual(shouldThey, shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent);
    }

    private static async Task<ShouldDefaultSecretsBeStored> ShouldDefaultSecretsBeStoredAsync(ISecretRepository sut, IErrorsAndInfos errorsAndInfos) {
        var secret = new SecretShouldDefaultSecretsBeStored();
        ShouldDefaultSecretsBeStored shouldDefaultSecretsBeStored = await sut.GetAsync(secret, errorsAndInfos);
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
        Assert.Contains(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase), errorsAndInfos.Errors, errorsAndInfos.ErrorsToString());
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
        Assert.Contains(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase), errorsAndInfos.Errors, errorsAndInfos.ErrorsToString());
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
        Assert.Contains(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase), errorsAndInfos.Errors, errorsAndInfos.ErrorsToString());
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
        CsLambda script = await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        const string addedString = "/* This script has been altered */";
        Assert.DoesNotContain(addedString, script.LambdaExpression);
        script.LambdaExpression = addedString + "\r\n" + script.LambdaExpression;
        await sut.SetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        sut.Values.Clear();
        await sut.ValueOrDefaultAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        script = (CsLambda)sut.Values[secret.Guid];
        Assert.StartsWith(addedString, script.LambdaExpression);
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
        string folder = SecretRepositoryFolder(true, false);
        Assert.IsEmpty(Directory.GetFiles(folder, secret.Guid + "*.*"));
        sut.SaveSample(secret, false);
        Assert.HasCount(1, Directory.GetFiles(folder, secret.Guid + "*.xml"));
        Assert.HasCount(1, Directory.GetFiles(folder, secret.Guid + "*.xsd"));
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
        string folder = SecretRepositoryFolder(true, false);
        Assert.IsEmpty(Directory.GetFiles(folder, secret.Guid + "*.*"));
        sut.SaveSample(secret, false);
        Assert.HasCount(1, Directory.GetFiles(folder, secret.Guid + "*.xml"));
        Assert.HasCount(1, Directory.GetFiles(folder, secret.Guid + "*.xsd"));
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
        LongString longSecretString = await sut.GetAsync(secret, errorsAndInfos);
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
        CrewMember valueOrDefault = secret.DefaultValue;
        string xml = Container.Resolve<IXmlSerializer>().Serialize(valueOrDefault).Replace("Crew", "Curfew");
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
        await sut.GetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        listOfElements = GetSecretListOfElements(sut, secret);
        Assert.HasCount(2, listOfElements);
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