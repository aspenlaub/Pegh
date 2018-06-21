using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class SecretRepositoryTest {
        private IComponentProvider ComponentProvider { get; set; }
        private IComponentProvider AlternativeComponentProvider { get; set; }
        private IFolder AppDataSpecialFolder { get; set; }
        private SecretRepository Sut { get; set; }
        private SecretRepository AlternativeSut { get; set; }
        private const string SomeFirstName = "Some First Name", SomeSurName = "Some Surname", SomeRank = "Some Rank";
        private const string Passphrase = "DbDy38Dk973-5DeC9-4A.10-A7$45-DB§66C15!!05B80";

        [TestInitialize]
        public void Initialize() {
            ComponentProvider = new ComponentProvider();
            AlternativeComponentProvider = new ComponentProvider();
            AppDataSpecialFolder = new Folder(Path.GetTempPath() + @"NoSecrets");
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
            Sut.IsUserPresent = false;
            Sut.PassphraseIfUserIsNotPresent = Passphrase;

            AlternativeSut = AlternativeComponentProvider.SecretRepository as SecretRepository;
            Assert.IsNotNull(AlternativeSut);
            AlternativeSut.IsUserPresent = false;
            AlternativeSut.PassphraseIfUserIsNotPresent = Passphrase;
        }

        [ClassCleanup]
        public static void ClassCleanup() {
            var folder = new Folder(Path.GetTempPath() + @"NoSecrets");
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
        public void CanGetDefault() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new SecretCrewMember();
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName };
            Sut.Reset(secret, false);
            Assert.IsNull(GetSecretCrewMember(secret));
            var crewMember = Sut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.AreEqual(SecretCrewMember.DefaultFirstName, crewMember.FirstName);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void ExistsAfterSetting() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            var errorsAndInfos = new ErrorsAndInfos();
            Sut.Set(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsTrue(Sut.Exists(secret, false));
            Sut.Reset(secret, false);
            Assert.IsFalse(Sut.Exists(secret, false));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void CanGetAfterSetting() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            var errorsAndInfos = new ErrorsAndInfos();
            Sut.Set(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Sut.Values.Remove(secret.Guid);
            Assert.IsNull(GetSecretCrewMember(secret));
            Sut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.AreEqual(SomeFirstName, GetSecretCrewMember(secret).FirstName);
            Assert.AreEqual(SomeSurName, GetSecretCrewMember(secret).SurName);
            Assert.AreEqual(SomeRank, GetSecretCrewMember(secret).Rank);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void CanGetScriptSecret() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret, false);
            var list = TestListOfStrings();
            var enumeratedList = Sut.ExecutePowershellFunction(Sut.Get(secret, errorsAndInfos), list);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsNotNull(enumeratedList);
            var i = 0;
            foreach (var s in enumeratedList) {
                Assert.AreEqual(list[i], s);
                i ++;
            }
        }

        [TestMethod]
        public void CannotGetScriptSecretIfScriptCannotBeRunWithoutErrors() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new FailingSecretStringListEnumerator();
            Sut.Reset(secret, false);
            var list = TestListOfStrings();
            var enumeratedList = Sut.ExecutePowershellFunction(Sut.Get(secret, errorsAndInfos), list);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsNull(enumeratedList);

            CleanUpSecretRepository(false);
        }

        private static List<string> TestListOfStrings() {
            var list = new List<string> {
                "This",
                "is",
                "not",
                "a",
                "list",
                Guid.NewGuid().ToString()
            };
            return list;
        }

        private void CleanUpSecretRepository(bool alternative) {
            var secrets = new List<IGuid> { new SecretCrewMember(), new SecretStringListEnumerator(), new FailingSecretStringListEnumerator(), new EncryptedSecretCrewMember(), new SecretListOfElements() };
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
        public void DefaultSecretIsSavedIfSecretSaysSo() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Sut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsTrue(Sut.Exists(secret, false));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void DefaultSecretIsNotSavedIfSecretSaysNo() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Sut.Get(secret, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("ecret has not been defined")), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsFalse(Sut.Exists(secret, false));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void DefaultSecretIsNotReturnedIfSecretSaysItShouldNotBeSaved() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Assert.IsNull(Sut.Get(secret, errorsAndInfos));
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("ecret has not been defined")), string.Join("\r\n", errorsAndInfos.Errors));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void DefaultSecretIsNotCachedIfSecretSaysItShouldNotBeSaved() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Sut.Get(secret, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("ecret has not been defined")), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsFalse(Sut.Values.ContainsKey(secret.Guid));
            CleanUpSecretRepository(false);
        }

        private void SetShouldDefaultSecretsBeStored(bool shouldThey, IErrorsAndInfos errorsAndInfos) {
            var shouldDefaultSecretsBeStored = ShouldDefaultSecretsBeStored(errorsAndInfos);
            if (shouldThey == shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent) {
                return;
            }

            shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent = shouldThey;
            shouldDefaultSecretsBeStored = ShouldDefaultSecretsBeStored(errorsAndInfos);
            Assert.AreEqual(shouldThey, shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent);
        }

        private ShouldDefaultSecretsBeStored ShouldDefaultSecretsBeStored(IErrorsAndInfos errorsAndInfos) {
            var secret = new SecretShouldDefaultSecretsBeStored();
            var shouldDefaultSecretsBeStored = Sut.Get(secret, errorsAndInfos);
            Assert.IsNotNull(shouldDefaultSecretsBeStored);
            return shouldDefaultSecretsBeStored;
        }

        [TestMethod]
        public void DefaultScriptSecretIsSavedIfSecretSaysSo() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret, false);
            Sut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsTrue(Sut.Exists(secret, false));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void DefaultScriptSecretIsNotSavedIfSecretSaysNo() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret, false);
            Sut.Get(secret, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("ecret has not been defined")), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsFalse(Sut.Exists(secret, false));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void DefaultScriptSecretIsNotReturnedIfSecretSaysItShouldNotBeSaved() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret, false);
            Assert.IsNull(Sut.Get(secret, errorsAndInfos));
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("ecret has not been defined")), string.Join("\r\n", errorsAndInfos.Errors));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void DefaultScriptSecretIsNotCachedIfSecretSaysItShouldNotBeSaved() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret, false);
            Sut.Get(secret, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("ecret has not been defined")), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsFalse(Sut.Values.ContainsKey(secret.Guid));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void SavedScriptSecretIsUsedDuringExecution() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret, false);
            var script = Sut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            const string addedString = "/* This script has been altered */";
            Assert.IsFalse(script.Script.StartsWith(addedString));
            script.Script = addedString + "\r\n" + script.Script;
            Sut.Set(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Sut.Values.Clear();
            Sut.ValueOrDefault(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            script = (PowershellFunction<IList<string>, IEnumerable<string>>)Sut.Values[secret.Guid];
            var scriptText = script.Script;
            Assert.IsTrue(scriptText.StartsWith(addedString));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void DefaultSecretSampleIsSavedIfSecretSaysSo() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
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
        public void DefaultSecretSampleIsSavedEvenIfSecretSaysNo() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(false, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
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
        public void CanGetLongSecretString() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new LongSecretString();
            var longSecretString = Sut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.AreEqual(128, longSecretString.TheLongString.Length);

            secret = new LongSecretString();
            Assert.AreEqual(128, secret.DefaultValue.TheLongString.Length);
            Assert.AreNotEqual(longSecretString.TheLongString, secret.DefaultValue.TheLongString);
        }

        [TestMethod]
        public void CanGetDefaultForEncryptedSecret() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var secret = new EncryptedSecretCrewMember();
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName };
            Sut.Reset(secret,  true);
            Assert.IsNull(GetSecretCrewMember(secret));
            Assert.IsFalse(Sut.Exists(secret, true));
            var crewMember = Sut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsTrue(Sut.Exists(secret, true));
            Assert.AreEqual(EncryptedSecretCrewMember.DefaultFirstName, crewMember.FirstName);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void CanGetEncryptedSecretAfterSetting() {
            var secret = new EncryptedSecretCrewMember();
            Sut.Reset(secret, true);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            var errorsAndInfos = new ErrorsAndInfos();
            Sut.Set(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Sut.Values.Remove(secret.Guid);
            Assert.IsNull(GetSecretCrewMember(secret));
            Sut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.AreEqual(SomeFirstName, GetSecretCrewMember(secret).FirstName);
            Assert.AreEqual(SomeSurName, GetSecretCrewMember(secret).SurName);
            Assert.AreEqual(SomeRank, GetSecretCrewMember(secret).Rank);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void CanGetDefaultForEncryptedSecretWithoutPassphraseButItIsNotSaved() {
            var errorsAndInfos = new ErrorsAndInfos();
            SetShouldDefaultSecretsBeStored(true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            Sut.PassphraseIfUserIsNotPresent = "";
            var secret = new EncryptedSecretCrewMember();
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName };
            Sut.Reset(secret, true);
            Assert.IsNull(GetSecretCrewMember(secret));
            Assert.IsFalse(Sut.Exists(secret, true));
            var crewMember = Sut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsFalse(Sut.Exists(secret, true));
            Assert.AreEqual(EncryptedSecretCrewMember.DefaultFirstName, crewMember.FirstName);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void CannotGetEncryptedSecretAfterSettingIfDisguiserFails() {
            var secret = new EncryptedSecretCrewMember();
            Sut.Reset(secret, true);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            var errorsAndInfos = new ErrorsAndInfos();
            Sut.Set(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            var componentProviderMock = new Mock<IComponentProvider>();
            componentProviderMock.Setup(c => c.PeghEnvironment).Returns(ComponentProvider.PeghEnvironment);
            componentProviderMock.Setup(c => c.XmlDeserializer).Returns(ComponentProvider.XmlDeserializer);
            componentProviderMock.Setup(c => c.XmlSerializer).Returns(ComponentProvider.XmlSerializer);
            componentProviderMock.Setup(c => c.XmlSchemer).Returns(ComponentProvider.XmlSchemer);
            componentProviderMock.Setup(c => c.PowershellExecuter).Returns(ComponentProvider.PowershellExecuter);
            var disguiserMock = new Mock<IDisguiser>();
            disguiserMock.Setup(d => d.Disguise(It.IsAny<string>(), It.IsAny<IErrorsAndInfos>())).Returns("");
            componentProviderMock.Setup(c => c.Disguiser).Returns(disguiserMock.Object);
            Sut = new SecretRepository(componentProviderMock.Object) {
                IsUserPresent = false,
                PassphraseIfUserIsNotPresent = Passphrase
            };
            Assert.IsNull(GetSecretCrewMember(secret));
            Sut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsNull(GetSecretCrewMember(secret));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void CannotGetEncryptedSecretAfterSettingIfPassphraseIsWrong() {
            var secret = new EncryptedSecretCrewMember();
            Sut.Reset(secret, true);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            var errorsAndInfos = new ErrorsAndInfos();
            Sut.Set(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Sut = new SecretRepository(ComponentProvider) {
                IsUserPresent = false,
                PassphraseIfUserIsNotPresent = Passphrase + Passphrase
            };
            Assert.IsNull(GetSecretCrewMember(secret));
            Sut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsNull(GetSecretCrewMember(secret));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void DoesNotExistAfterTryingToSaveInvalidXml() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            var valueOrDefault = secret.DefaultValue;
            var xml = ComponentProvider.XmlSerializer.Serialize(valueOrDefault).Replace("Crew", "Curfew");
            var errorsAndInfos = new ErrorsAndInfos();
            Sut.WriteToFile(secret, xml, false, false, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.All(e => e.Contains("The \'http://www.aspenlaub.net:CurfewMember\' element is not declared")), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsFalse(Sut.Exists(secret, false));
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void CanSetAndGetSecretListOfElements() {
            var secret = new SecretListOfElements();
            Sut.Reset(secret, false);
            var listOfElements = new ListOfElements { new ListElement { Value = "One" }, new ListElement { Value = "Two" }};
            Sut.Values[secret.Guid] = listOfElements;
            var errorsAndInfos = new ErrorsAndInfos();
            Sut.Set(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Sut = new SecretRepository(ComponentProvider) {
                IsUserPresent = false,
                PassphraseIfUserIsNotPresent = Passphrase + Passphrase
            };
            Sut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            listOfElements = GetSecretListOfElements(secret);
            Assert.AreEqual(2, listOfElements.Count);
            Assert.AreEqual("Two", listOfElements[1].Value);
            CleanUpSecretRepository(false);
        }

        [TestMethod]
        public void CanWorkWithAlternativePeghEnvironment() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            var errorsAndInfos = new ErrorsAndInfos();
            Sut.Set(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            secret = new SecretCrewMember();
            AlternativeSut.Reset(secret, false);
            AlternativeSut.Values[secret.Guid] = new CrewMember { FirstName = "ALT " + SomeFirstName, SurName = "ALT" + SomeSurName, Rank = "ALT" + SomeRank };
            errorsAndInfos = new ErrorsAndInfos();
            AlternativeSut.Set(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));

            Sut.Values.Remove(secret.Guid);
            AlternativeSut.Values.Remove(secret.Guid);

            AlternativeSut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.AreEqual("ALT " + SomeFirstName, GetAlternativeSecretCrewMember(secret).FirstName);

            Sut.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.AreEqual(SomeFirstName, GetSecretCrewMember(secret).FirstName);

            CleanUpSecretRepository(false);
            CleanUpSecretRepository(true);
        }
    }
}