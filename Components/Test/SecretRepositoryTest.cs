using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.TestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class SecretRepositoryTest {
        private IComponentProvider ComponentProvider { get; set; }
        private SecretRepository Sut { get; set; }
        private const string SomeFirstName = "Some First Name", SomeSurName = "Some Surname", SomeRank = "Some Rank";
        private const string Passphrase = "DbDy38Dk973-5DeC9-4A.10-A7$45-DB§66C15!!05B80";

        [TestInitialize]
        public void Initialize() {
            ComponentProvider = new ComponentProvider();
            SecretRepositoryFolder(false);
            SecretRepositoryFolder(true);
            Sut = ComponentProvider.SecretRepository as SecretRepository;
            Assert.IsNotNull(Sut);
            Sut.IsUserPresent = false;
            Sut.PassphraseIfUserIsNotPresent = Passphrase;
        }

        private string SecretRepositoryFolder(bool sample) {
            var folder = ComponentProvider.PeghEnvironment.RootWorkFolder + (sample ? @"\SecretSamples" : @"\SecretRepository");
            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        protected CrewMember GetSecretCrewMember(IGuid secret) {
            return Sut.Values.ContainsKey(secret.Guid) ? Sut.Values[secret.Guid] as CrewMember : null;
        }

        [TestMethod]
        public void DoesNotExistInRepositoryAfterRemoval() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Assert.IsFalse(Sut.Exists(secret, false));
        }

        [TestMethod]
        public void CanGetDefault() {
            SetShouldDefaultSecretsBeStored(true);
            var secret = new SecretCrewMember();
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName };
            Sut.Reset(secret, false);
            Assert.IsNull(GetSecretCrewMember(secret));
            var crewMember = Sut.Get(secret);
            Assert.AreEqual(SecretCrewMember.DefaultFirstName, crewMember.FirstName);
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void ExistsAfterSetting() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Sut.Set(secret);
            Assert.IsTrue(Sut.Exists(secret, false));
            Sut.Reset(secret, false);
            Assert.IsFalse(Sut.Exists(secret, false));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void CanGetAfterSetting() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            Sut.Set(secret);
            Sut.Values.Remove(secret.Guid);
            Assert.IsNull(GetSecretCrewMember(secret));
            Sut.Get(secret);
            Assert.AreEqual(SomeFirstName, GetSecretCrewMember(secret).FirstName);
            Assert.AreEqual(SomeSurName, GetSecretCrewMember(secret).SurName);
            Assert.AreEqual(SomeRank, GetSecretCrewMember(secret).Rank);
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void CanGetScriptSecret() {
            SetShouldDefaultSecretsBeStored(true);

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret, false);
            var list = TestListOfStrings();
            var enumeratedList = Sut.ExecutePowershellFunction(Sut.Get(secret), list);
            Assert.IsNotNull(enumeratedList);
            var i = 0;
            foreach (var s in enumeratedList) {
                Assert.AreEqual(list[i], s);
                i ++;
            }
        }

        [TestMethod]
        public void CannotGetScriptSecretIfScriptCannotBeRunWithoutErrors() {
            SetShouldDefaultSecretsBeStored(true);

            var secret = new FailingSecretStringListEnumerator();
            Sut.Reset(secret, false);
            var list = TestListOfStrings();
            var enumeratedList = Sut.ExecutePowershellFunction(Sut.Get(secret), list);
            Assert.IsNull(enumeratedList);

            CleanUpSecretRepository();
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

        private void CleanUpSecretRepository() {
            var secrets = new List<IGuid> { new SecretCrewMember(), new SecretStringListEnumerator(), new FailingSecretStringListEnumerator(), new EncryptedSecretCrewMember() };
            foreach (var files in new[] { false, true }.Select(sample => SecretRepositoryFolder(sample)).SelectMany(folder => secrets.Select(secret => Directory.GetFiles(folder, secret.Guid + "*.*").ToList()))) {
                Assert.IsTrue(files.Count < 2);
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
            SetShouldDefaultSecretsBeStored(true);

            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Sut.Get(secret);
            Assert.IsTrue(Sut.Exists(secret, false));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultSecretIsNotSavedIfSecretSaysNo() {
            SetShouldDefaultSecretsBeStored(false);

            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Sut.Get(secret);
            Assert.IsFalse(Sut.Exists(secret, false));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultSecretIsNotReturnedIfSecretSaysItShouldNotBeSaved() {
            SetShouldDefaultSecretsBeStored(false);

            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Assert.IsNull(Sut.Get(secret));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultSecretIsNotCachedIfSecretSaysItShouldNotBeSaved() {
            SetShouldDefaultSecretsBeStored(false);

            var secret = new SecretCrewMember();
            Sut.Reset(secret, false);
            Sut.Get(secret);
            Assert.IsFalse(Sut.Values.ContainsKey(secret.Guid));
            CleanUpSecretRepository();
        }

        private void SetShouldDefaultSecretsBeStored(bool shouldThey) {
            var shouldDefaultSecretsBeStored = ShouldDefaultSecretsBeStored();
            if (shouldThey == shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent) {
                return;
            }

            shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent = shouldThey;
            shouldDefaultSecretsBeStored = ShouldDefaultSecretsBeStored();
            Assert.AreEqual(shouldThey, shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent);
        }

        private ShouldDefaultSecretsBeStored ShouldDefaultSecretsBeStored() {
            var secret = new SecretShouldDefaultSecretsBeStored();
            var shouldDefaultSecretsBeStored = Sut.Get(secret);
            Assert.IsNotNull(shouldDefaultSecretsBeStored);
            return shouldDefaultSecretsBeStored;
        }

        [TestMethod]
        public void DefaultScriptSecretIsSavedIfSecretSaysSo() {
            SetShouldDefaultSecretsBeStored(true);

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret, false);
            Sut.Get(secret);
            Assert.IsTrue(Sut.Exists(secret, false));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultScriptSecretIsNotSavedIfSecretSaysNo() {
            SetShouldDefaultSecretsBeStored(false);

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret, false);
            Sut.Get(secret);
            Assert.IsFalse(Sut.Exists(secret, false));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultScriptSecretIsNotReturnedIfSecretSaysItShouldNotBeSaved() {
            SetShouldDefaultSecretsBeStored(false);

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret, false);
            Assert.IsNull(Sut.Get(secret));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultScriptSecretIsNotCachedIfSecretSaysItShouldNotBeSaved() {
            SetShouldDefaultSecretsBeStored(false);

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret, false);
            Sut.Get(secret);
            Assert.IsFalse(Sut.Values.ContainsKey(secret.Guid));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void SavedScriptSecretIsUsedDuringExecution() {
            SetShouldDefaultSecretsBeStored(true);

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret, false);
            var script = Sut.Get(secret);
            const string addedString = "/* This script has been altered */";
            Assert.IsFalse(script.Script.StartsWith(addedString));
            script.Script = addedString + "\r\n" + script.Script;
            Sut.Set(secret);
            Sut.Values.Clear();
            Sut.ValueOrDefault(secret);
            script = (PowershellFunction<IList<string>, IEnumerable<string>>)Sut.Values[secret.Guid];
            var scriptText = script.Script;
            Assert.IsTrue(scriptText.StartsWith(addedString));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultSecretSampleIsSavedIfSecretSaysSo() {
            SetShouldDefaultSecretsBeStored(true);
            CleanUpSecretRepository();

            var secret = new SecretCrewMember();
            var folder = SecretRepositoryFolder(true);
            Assert.AreEqual(0, Directory.GetFiles(folder, secret.Guid + "*.*").Length);
            Sut.SaveSample(secret);
            Assert.AreEqual(1, Directory.GetFiles(folder, secret.Guid + "*.*").Length);
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultSecretSampleIsSavedEvenIfSecretSaysNo() {
            SetShouldDefaultSecretsBeStored(false);
            CleanUpSecretRepository();

            var secret = new SecretCrewMember();
            var folder = SecretRepositoryFolder(true);
            Assert.AreEqual(0, Directory.GetFiles(folder, secret.Guid + "*.*").Length);
            Sut.SaveSample(secret);
            Assert.AreEqual(1, Directory.GetFiles(folder, secret.Guid + "*.*").Length);
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void CanGetLongSecretString() {
            SetShouldDefaultSecretsBeStored(true);

            var secret = new LongSecretString();
            var longSecretString = Sut.Get(secret);
            Assert.AreEqual(128, longSecretString.TheLongString.Length);

            secret = new LongSecretString();
            Assert.AreEqual(128, secret.DefaultValue.TheLongString.Length);
            Assert.AreNotEqual(longSecretString.TheLongString, secret.DefaultValue.TheLongString);
        }

        [TestMethod]
        public void CanGetDefaultForEncryptedSecret() {
            SetShouldDefaultSecretsBeStored(true);
            var secret = new EncryptedSecretCrewMember();
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName };
            Sut.Reset(secret,  true);
            Assert.IsNull(GetSecretCrewMember(secret));
            Assert.IsFalse(Sut.Exists(secret, true));
            var crewMember = Sut.Get(secret);
            Assert.IsTrue(Sut.Exists(secret, true));
            Assert.AreEqual(EncryptedSecretCrewMember.DefaultFirstName, crewMember.FirstName);
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void CanGetEncryptedSecretAfterSetting() {
            var secret = new EncryptedSecretCrewMember();
            Sut.Reset(secret, true);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            Sut.Set(secret);
            Sut.Values.Remove(secret.Guid);
            Assert.IsNull(GetSecretCrewMember(secret));
            Sut.Get(secret);
            Assert.AreEqual(SomeFirstName, GetSecretCrewMember(secret).FirstName);
            Assert.AreEqual(SomeSurName, GetSecretCrewMember(secret).SurName);
            Assert.AreEqual(SomeRank, GetSecretCrewMember(secret).Rank);
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void CanGetDefaultForEncryptedSecretWithoutPassphraseButItIsNotSaved() {
            SetShouldDefaultSecretsBeStored(true);
            Sut.PassphraseIfUserIsNotPresent = "";
            var secret = new EncryptedSecretCrewMember();
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName };
            Sut.Reset(secret, true);
            Assert.IsNull(GetSecretCrewMember(secret));
            Assert.IsFalse(Sut.Exists(secret, true));
            var crewMember = Sut.Get(secret);
            Assert.IsFalse(Sut.Exists(secret, true));
            Assert.AreEqual(EncryptedSecretCrewMember.DefaultFirstName, crewMember.FirstName);
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void CannotGetEncryptedSecretAfterSettingIfDisguiserFails() {
            var secret = new EncryptedSecretCrewMember();
            Sut.Reset(secret, true);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            Sut.Set(secret);

            var componentProviderMock = new Mock<IComponentProvider>();
            componentProviderMock.Setup(c => c.PeghEnvironment).Returns(ComponentProvider.PeghEnvironment);
            componentProviderMock.Setup(c => c.XmlDeserializer).Returns(ComponentProvider.XmlDeserializer);
            componentProviderMock.Setup(c => c.XmlSerializer).Returns(ComponentProvider.XmlSerializer);
            var disguiserMock = new Mock<IDisguiser>();
            disguiserMock.Setup(d => d.Disguise(It.IsAny<string>())).Returns("");
            componentProviderMock.Setup(c => c.Disguiser).Returns(disguiserMock.Object);
            Sut = new SecretRepository(componentProviderMock.Object) {
                IsUserPresent = false,
                PassphraseIfUserIsNotPresent = Passphrase
            };
            Assert.IsNull(GetSecretCrewMember(secret));
            Sut.Get(secret);
            Assert.IsNull(GetSecretCrewMember(secret));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void CannotGetEncryptedSecretAfterSettingIfPassphraseIsWrong() {
            var secret = new EncryptedSecretCrewMember();
            Sut.Reset(secret, true);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            Sut.Set(secret);
            Sut = new SecretRepository(ComponentProvider) {
                IsUserPresent = false,
                PassphraseIfUserIsNotPresent = Passphrase + Passphrase
            };
            Assert.IsNull(GetSecretCrewMember(secret));
            Sut.Get(secret);
            Assert.IsNull(GetSecretCrewMember(secret));
            CleanUpSecretRepository();
        }
    }
}