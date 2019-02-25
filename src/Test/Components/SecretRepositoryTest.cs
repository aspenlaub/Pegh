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
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    [TestClass]
    public class SecretRepositoryTest {
        protected IComponentProvider ComponentProvider { get; set; }
        protected IComponentProvider AlternativeComponentProvider { get; set; }
        protected IFolder AppDataSpecialFolder { get; set; }
        protected SecretRepository Sut { get; set; }
        protected SecretRepository AlternativeSut { get; set; }
        protected const string SomeFirstName = "Some First Name", SomeSurName = "Some Surname", SomeRank = "Some Rank";
        protected const string Passphrase = "DbDy38Dk973-5DeC9-4A.10-A7$45-DB§66C15!!05B80";
        protected Mock<ICsArgumentPrompter> CsArgumentPrompterMock, AlternativeCsArgumentPrompterMock;

        [TestInitialize]
        public void Initialize() {
            ComponentProvider = new ComponentProvider();
            AlternativeComponentProvider = new ComponentProvider();
            AppDataSpecialFolder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder("NoSecrets");
            if (AppDataSpecialFolder.Exists()) {
                var deleter = new FolderDeleter();
                deleter.DeleteFolder(AppDataSpecialFolder);
            }
            AppDataSpecialFolder.CreateIfNecessary();
            AlternativeComponentProvider.SetAppDataSpecialFolder(AppDataSpecialFolder);

            SecretRepositoryFolder(false, false);
            SecretRepositoryFolder(true, false);
            SecretRepositoryFolder(false, true);
            SecretRepositoryFolder(true, true);

            Sut = ComponentProvider.SecretRepository as SecretRepository;
            Assert.IsNotNull(Sut);
            CsArgumentPrompterMock = new Mock<ICsArgumentPrompter>();
            CsArgumentPrompterMock.Setup(p => p.PromptForArgument(It.IsAny<string>(), It.IsAny<string>())).Returns(Passphrase);
            Sut.CsArgumentPrompter = CsArgumentPrompterMock.Object;

            AlternativeSut = AlternativeComponentProvider.SecretRepository as SecretRepository;
            Assert.IsNotNull(AlternativeSut);
            AlternativeCsArgumentPrompterMock = new Mock<ICsArgumentPrompter>();
            AlternativeCsArgumentPrompterMock.Setup(p => p.PromptForArgument(It.IsAny<string>(), It.IsAny<string>())).Returns(Passphrase);
            AlternativeSut.CsArgumentPrompter = AlternativeCsArgumentPrompterMock.Object;
        }

        [ClassCleanup]
        public static void ClassCleanup() {
            var folder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder("NoSecrets");
            if (!folder.Exists()) { return; }


            var deleter = new FolderDeleter();
            deleter.DeleteFolder(folder);
        }

        private string SecretRepositoryFolder(bool sample, bool alternative) {
            var componentProvider = alternative ? AlternativeComponentProvider : ComponentProvider;
            var folder = componentProvider.PeghEnvironment.RootWorkFolder + (sample ? @"\SecretSamples" : @"\SecretRepository");
            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        protected CrewMember GetSecretCrewMember(IGuid secret) {
            return Sut.Values.ContainsKey(secret.Guid) ? Sut.Values[secret.Guid] as CrewMember : null;
        }

        protected CrewMember GetAlternativeSecretCrewMember(IGuid secret) {
            return AlternativeSut.Values.ContainsKey(secret.Guid) ? AlternativeSut.Values[secret.Guid] as CrewMember : null;
        }

        protected ListOfElements GetSecretListOfElements(IGuid secret) {
            return Sut.Values.ContainsKey(secret.Guid) ? Sut.Values[secret.Guid] as ListOfElements : null;
        }

        [TestMethod]
        public void DoesNotExistInRepositoryAfterRemoval() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Assert.IsFalse(Sut.Exists(secret, false));
        }

        [TestMethod]
        public async Task CanGetDefault() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new SecretCrewMember();
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName };
            Sut.Reset(secret, false);
            Assert.IsNull(GetSecretCrewMember(secret));
            var crewMember = await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.AreEqual(SecretCrewMember.DefaultFirstName, crewMember.FirstName);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task ExistsAfterSetting() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            var errorsAndInfos = new ErrorsAndInfos();
            await Sut.SetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.IsTrue(Sut.Exists(secret, false));
            Sut.Reset(secret, false);
            Assert.IsFalse(Sut.Exists(secret, false));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task CanGetAfterSetting() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            var errorsAndInfos = new ErrorsAndInfos();
            await Sut.SetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Sut.Values.Remove(secret.Guid);
            Assert.IsNull(GetSecretCrewMember(secret));
            await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.AreEqual(SomeFirstName, GetSecretCrewMember(secret).FirstName);
            Assert.AreEqual(SomeSurName, GetSecretCrewMember(secret).SurName);
            Assert.AreEqual(SomeRank, GetSecretCrewMember(secret).Rank);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task CanGetScriptSecret() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new SecretStringFunction();
            Sut.Reset(secret, false);
            const string s = "This is not a string";
            var r = (await Sut.CompileCsLambdaAsync<string, string>(await Sut.GetAsync(secret, errorsAndInfos)))(s);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.IsTrue(r.StartsWith(s));
            Assert.IsTrue(r.Contains("with greetings from a csx"));
        }

        private void CleanUpSecretRepository(bool alternative) {
            var secrets = new List<IGuid> { new SecretCrewMember(), new SecretStringFunction(), new EncryptedSecretCrewMember(), new SecretListOfElements() };
            foreach (var files in new[] { false, true }.Select(sample => SecretRepositoryFolder(sample, alternative)).SelectMany(folder => secrets.Select(secret => Directory.GetFiles(folder, secret.Guid + "*.*").ToList()))) {
                Assert.IsTrue(files.Count < 3);
                files.ForEach(f => File.Delete(f));
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
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.IsTrue(Sut.Exists(secret, false));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task DefaultSecretIsNotSavedIfSecretSaysNo() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase)), errorsAndInfos.ErrorsToString());
            Assert.IsFalse(Sut.Exists(secret, false));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task DefaultSecretIsNotReturnedIfSecretSaysItShouldNotBeSaved() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Assert.IsNull(await Sut.GetAsync(secret, errorsAndInfos));
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase)), errorsAndInfos.ErrorsToString());
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task DefaultSecretIsNotCachedIfSecretSaysItShouldNotBeSaved() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase)), errorsAndInfos.ErrorsToString());
            Assert.IsFalse(Sut.Values.ContainsKey(secret.Guid));
            CleanUpSecretRepository(false);
        }

        private async Task SetShouldDefaultSecretsBeStored(bool shouldThey, IErrorsAndInfos errorsAndInfos) {
            var shouldDefaultSecretsBeStored = await ShouldDefaultSecretsBeStoredAsync(errorsAndInfos);
            if (shouldThey == shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent) {
                return;
            }

            shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent = shouldThey;
            shouldDefaultSecretsBeStored = await ShouldDefaultSecretsBeStoredAsync(errorsAndInfos);
            Assert.AreEqual(shouldThey, shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent);
        }

        private async Task<ShouldDefaultSecretsBeStored> ShouldDefaultSecretsBeStoredAsync(IErrorsAndInfos errorsAndInfos) {
            var secret = new SecretShouldDefaultSecretsBeStored();
            var shouldDefaultSecretsBeStored = await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsNotNull(shouldDefaultSecretsBeStored);
            return shouldDefaultSecretsBeStored;
        }

        [TestMethod]
        public async Task DefaultScriptSecretIsSavedIfSecretSaysSo() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new SecretStringFunction();
            Sut.Reset(secret, false);
            await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.IsTrue(Sut.Exists(secret, false));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task DefaultScriptSecretIsNotSavedIfSecretSaysNo() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new SecretStringFunction();
            Sut.Reset(secret, false);
            await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase)), errorsAndInfos.ErrorsToString());
            Assert.IsFalse(Sut.Exists(secret, false));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task DefaultScriptSecretIsNotReturnedIfSecretSaysItShouldNotBeSaved() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new SecretStringFunction();
            Sut.Reset(secret, false);
            Assert.IsNull(await Sut.GetAsync(secret, errorsAndInfos));
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase)), errorsAndInfos.ErrorsToString());
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task DefaultScriptSecretIsNotCachedIfSecretSaysItShouldNotBeSaved() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new SecretStringFunction();
            Sut.Reset(secret, false);
            await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("Secret has not been defined", StringComparison.InvariantCultureIgnoreCase)), errorsAndInfos.ErrorsToString());
            Assert.IsFalse(Sut.Values.ContainsKey(secret.Guid));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task SavedScriptSecretIsUsedDuringExecution() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new SecretStringFunction();
            Sut.Reset(secret, false);
            var script = await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            const string addedString = "/* This script has been altered */";
            Assert.IsFalse(script.LambdaExpression.Contains(addedString));
            script.LambdaExpression = addedString + "\r\n" + script.LambdaExpression;
            await Sut.SetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Sut.Values.Clear();
            await Sut.ValueOrDefaultAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            script = (CsLambda)Sut.Values[secret.Guid];
            Assert.IsTrue(script.LambdaExpression.StartsWith(addedString));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task DefaultSecretSampleIsSavedIfSecretSaysSo() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            CleanUpSecretRepository(false);

            var secret = new SecretCrewMember();
            var folder = SecretRepositoryFolder(true, false);
            Assert.AreEqual(0, Directory.GetFiles(folder, secret.Guid + "*.*").Length);
            Sut.SaveSample(secret, false);
            Assert.AreEqual(1, Directory.GetFiles(folder, secret.Guid + "*.xml").Length);
            Assert.AreEqual(1, Directory.GetFiles(folder, secret.Guid + "*.xsd").Length);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task DefaultSecretSampleIsSavedEvenIfSecretSaysNo() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            CleanUpSecretRepository(false);

            var secret = new SecretCrewMember();
            var folder = SecretRepositoryFolder(true, false);
            Assert.AreEqual(0, Directory.GetFiles(folder, secret.Guid + "*.*").Length);
            Sut.SaveSample(secret, false);
            Assert.AreEqual(1, Directory.GetFiles(folder, secret.Guid + "*.xml").Length);
            Assert.AreEqual(1, Directory.GetFiles(folder, secret.Guid + "*.xsd").Length);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task CanGetLongSecretString() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new LongSecretString();
            var longSecretString = await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.AreEqual(128, longSecretString.TheLongString.Length);

            secret = new LongSecretString();
            Assert.AreEqual(128, secret.DefaultValue.TheLongString.Length);
            Assert.AreNotEqual(longSecretString.TheLongString, secret.DefaultValue.TheLongString);
        }

        [TestMethod]
        public async Task CanGetDefaultForEncryptedSecret() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new EncryptedSecretCrewMember();
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName };
            Sut.Reset(secret,  true);
            Assert.IsNull(GetSecretCrewMember(secret));
            Assert.IsFalse(Sut.Exists(secret, true));
            var crewMember = await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.IsTrue(Sut.Exists(secret, true));
            Assert.AreEqual(EncryptedSecretCrewMember.DefaultFirstName, crewMember.FirstName);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task CanGetEncryptedSecretAfterSetting() {
            var secret = new EncryptedSecretCrewMember();
            Sut.Reset(secret, true);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            var errorsAndInfos = new ErrorsAndInfos();
            await Sut.SetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Sut.Values.Remove(secret.Guid);
            Assert.IsNull(GetSecretCrewMember(secret));
            await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.AreEqual(SomeFirstName, GetSecretCrewMember(secret).FirstName);
            Assert.AreEqual(SomeSurName, GetSecretCrewMember(secret).SurName);
            Assert.AreEqual(SomeRank, GetSecretCrewMember(secret).Rank);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task CanGetDefaultForEncryptedSecretWithEmptyPassphraseButItIsNotSaved() {
            var errorsAndInfos = new ErrorsAndInfos();
            await SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var secret = new EncryptedSecretCrewMember();
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName };
            Sut.Reset(secret, true);
            Assert.IsNull(GetSecretCrewMember(secret));
            Assert.IsFalse(Sut.Exists(secret, true));
            CsArgumentPrompterMock.Setup(p => p.PromptForArgument(It.IsAny<string>(), It.IsAny<string>())).Returns("");
            var crewMember = await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.IsFalse(Sut.Exists(secret, true));
            Assert.AreEqual(EncryptedSecretCrewMember.DefaultFirstName, crewMember.FirstName);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task CannotGetEncryptedSecretAfterSettingIfDisguiserFails() {
            var secret = new EncryptedSecretCrewMember();
            Sut.Reset(secret, true);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            var errorsAndInfos = new ErrorsAndInfos();
            await Sut.SetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            var componentProviderMock = new Mock<IComponentProvider>();
            componentProviderMock.Setup(c => c.PeghEnvironment).Returns(ComponentProvider.PeghEnvironment);
            componentProviderMock.Setup(c => c.XmlDeserializer).Returns(ComponentProvider.XmlDeserializer);
            componentProviderMock.Setup(c => c.XmlSerializer).Returns(ComponentProvider.XmlSerializer);
            componentProviderMock.Setup(c => c.XmlSchemer).Returns(ComponentProvider.XmlSchemer);
            var disguiserMock = new Mock<IDisguiser>();
            disguiserMock.Setup(d => d.Disguise(It.IsAny<string>(), It.IsAny<IErrorsAndInfos>())).Returns(Task.FromResult(""));
            componentProviderMock.Setup(c => c.Disguiser).Returns(disguiserMock.Object);
            Sut = new SecretRepository(componentProviderMock.Object) {
                CsArgumentPrompter = CsArgumentPrompterMock.Object
            };
            Assert.IsNull(GetSecretCrewMember(secret));
            await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.IsNull(GetSecretCrewMember(secret));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task CannotGetEncryptedSecretAfterSettingIfPassphraseIsWrong() {
            var secret = new EncryptedSecretCrewMember();
            Sut.Reset(secret, true);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            var errorsAndInfos = new ErrorsAndInfos();
            await Sut.SetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            CsArgumentPrompterMock.Setup(p => p.PromptForArgument(It.IsAny<string>(), It.IsAny<string>())).Returns("Wrong-Password");
            Sut = new SecretRepository(ComponentProvider) {
                CsArgumentPrompter = CsArgumentPrompterMock.Object
            };
            Assert.IsNull(GetSecretCrewMember(secret));
            try {
                await Sut.GetAsync(secret, errorsAndInfos);
                throw new Exception("ZipException expected");
            } catch (ZipException e) {
                Assert.IsTrue(e.Message.Contains("Invalid password", StringComparison.InvariantCultureIgnoreCase));
            }

            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.IsNull(GetSecretCrewMember(secret));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task DoesNotExistAfterTryingToSaveInvalidXml() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            var valueOrDefault = secret.DefaultValue;
            var xml = ComponentProvider.XmlSerializer.Serialize(valueOrDefault).Replace("Crew", "Curfew");
            var errorsAndInfos = new ErrorsAndInfos();
            await Sut.WriteToFileAsync(secret, xml, false, false, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.All(e => e.Contains("The \'http://www.aspenlaub.net:CurfewMember\' element is not declared")), errorsAndInfos.ErrorsToString());
            Assert.IsFalse(Sut.Exists(secret, false));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task CanSetAndGetSecretListOfElements() {
            var secret = new SecretListOfElements();
            Sut.Reset(secret, false);
            var listOfElements = new ListOfElements { new ListElement { Value = "One" }, new ListElement { Value = "Two" }};
            Sut.Values[secret.Guid] = listOfElements;
            var errorsAndInfos = new ErrorsAndInfos();
            await Sut.SetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            CsArgumentPrompterMock.Setup(p => p.PromptForArgument(It.IsAny<string>(), It.IsAny<string>())).Returns(Passphrase + Passphrase);
            Sut = new SecretRepository(ComponentProvider) {
                CsArgumentPrompter = CsArgumentPrompterMock.Object
            };
            await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            listOfElements = GetSecretListOfElements(secret);
            Assert.AreEqual(2, listOfElements.Count);
            Assert.AreEqual("Two", listOfElements[1].Value);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public async Task CanWorkWithAlternativePeghEnvironment() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            var errorsAndInfos = new ErrorsAndInfos();
            await Sut.SetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            secret = new SecretCrewMember();
            AlternativeSut.Reset(secret, false);
            AlternativeSut.Values[secret.Guid] = new CrewMember { FirstName = "ALT " + SomeFirstName, SurName = "ALT" + SomeSurName, Rank = "ALT" + SomeRank };
            errorsAndInfos = new ErrorsAndInfos();
            await AlternativeSut.SetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());

            Sut.Values.Remove(secret.Guid);
            AlternativeSut.Values.Remove(secret.Guid);

            await AlternativeSut.GetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.AreEqual("ALT " + SomeFirstName, GetAlternativeSecretCrewMember(secret).FirstName);

            await Sut.GetAsync(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
            Assert.AreEqual(SomeFirstName, GetSecretCrewMember(secret).FirstName);

            CleanUpSecretRepository(false);
            CleanUpSecretRepository(true);
        }
    }
}