using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.TestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class SecretRepositoryTest {
        private IComponentProvider ComponentProvider { get; set; }
        private SecretRepository Sut { get; set; }
        private const string SomeFirstName = "Some First Name", SomeSurName = "Some Surname", SomeRank = "Some Rank";

        [TestInitialize]
        public void Initialize() {
            ComponentProvider = new ComponentProvider();
            Sut = ComponentProvider.SecretRepository as SecretRepository;
            SecretRepositoryFolder(false);
            SecretRepositoryFolder(true);
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
            Sut.Reset(secret);
            Assert.IsFalse(Sut.Exists(secret));
        }

        [TestMethod]
        public void CanGetDefault() {
            SetShouldDefaultSecretsBeStored(true);
            var secret = new SecretCrewMember();
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName };
            Sut.Reset(secret);
            Assert.IsNull(GetSecretCrewMember(secret));
            var crewMember = Sut.Get(secret);
            Assert.AreEqual(SecretCrewMember.DefaultFirstName, crewMember.FirstName);
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void ExistsAfterSetting() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret);
            Sut.Set(secret);
            Assert.IsTrue(Sut.Exists(secret));
            Sut.Reset(secret);
            Assert.IsFalse(Sut.Exists(secret));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void CanGetAfterSetting() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret);
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
            Sut.Reset(secret);
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
            Sut.Reset(secret);
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
            var secrets = new List<IGuid> { new SecretCrewMember(), new SecretStringListEnumerator(), new FailingSecretStringListEnumerator() };
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
            Sut.Reset(secret);
            Sut.Get(secret);
            Assert.IsTrue(Sut.Exists(secret));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultSecretIsNotSavedIfSecretSaysNo() {
            SetShouldDefaultSecretsBeStored(false);

            var secret = new SecretCrewMember();
            Sut.Reset(secret);
            Sut.Get(secret);
            Assert.IsFalse(Sut.Exists(secret));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultSecretIsNotReturnedIfSecretSaysItShouldNotBeSaved() {
            SetShouldDefaultSecretsBeStored(false);

            var secret = new SecretCrewMember();
            Sut.Reset(secret);
            Assert.IsNull(Sut.Get(secret));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultSecretIsNotCachedIfSecretSaysItShouldNotBeSaved() {
            SetShouldDefaultSecretsBeStored(false);

            var secret = new SecretCrewMember();
            Sut.Reset(secret);
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
            Sut.Reset(secret);
            Sut.Get(secret);
            Assert.IsTrue(Sut.Exists(secret));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultScriptSecretIsNotSavedIfSecretSaysNo() {
            SetShouldDefaultSecretsBeStored(false);

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret);
            Sut.Get(secret);
            Assert.IsFalse(Sut.Exists(secret));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultScriptSecretIsNotReturnedIfSecretSaysItShouldNotBeSaved() {
            SetShouldDefaultSecretsBeStored(false);

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret);
            Assert.IsNull(Sut.Get(secret));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void DefaultScriptSecretIsNotCachedIfSecretSaysItShouldNotBeSaved() {
            SetShouldDefaultSecretsBeStored(false);

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret);
            Sut.Get(secret);
            Assert.IsFalse(Sut.Values.ContainsKey(secret.Guid));
            CleanUpSecretRepository();
        }

        [TestMethod]
        public void SavedScriptSecretIsUsedDuringExecution() {
            SetShouldDefaultSecretsBeStored(true);

            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret);
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
    }
}