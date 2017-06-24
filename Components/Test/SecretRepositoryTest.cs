using System;
using System.Collections.Generic;
using System.IO;
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
            var folder = ComponentProvider.PeghEnvironment.RootWorkFolder + @"\SecretRepository";
            if (Directory.Exists(folder)) { return; }

            Directory.CreateDirectory(folder);
        }

        protected CrewMember GetSecretCrewMember(IGuid secret) {
            return Sut.Values[secret.Guid] as CrewMember;
        }

        [TestMethod]
        public void DoesNotExistInRepositoryAfterRemoval() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret);
            Assert.IsFalse(Sut.Exists(secret));
        }

        [TestMethod]
        public void CanGetDefault() {
            var secret = new SecretCrewMember();
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName };
            Sut.Reset(secret);
            Assert.AreEqual(SomeFirstName, GetSecretCrewMember(secret).FirstName);
            Sut.Get(secret);
            Assert.AreEqual(SecretCrewMember.DefaultFirstName, GetSecretCrewMember(secret).FirstName);
        }

        [TestMethod]
        public void ExistsAfterSetting() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret);
            Sut.Set(secret);
            Assert.IsTrue(Sut.Exists(secret));
        }

        [TestMethod]
        public void CanGetAfterSetting() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret);
            Sut.Values[secret.Guid] = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            Sut.Set(secret);
            Sut.Values[secret.Guid] = new CrewMember();
            Assert.IsNull(GetSecretCrewMember(secret).FirstName);
            Assert.IsNull(GetSecretCrewMember(secret).SurName);
            Assert.IsNull(GetSecretCrewMember(secret).Rank);
            Sut.Get(secret);
            Assert.AreEqual(SomeFirstName, GetSecretCrewMember(secret).FirstName);
            Assert.AreEqual(SomeSurName, GetSecretCrewMember(secret).SurName);
            Assert.AreEqual(SomeRank, GetSecretCrewMember(secret).Rank);
        }

        [TestMethod]
        public void CanGetScriptSecret() {
            var secret = new SecretStringListEnumerator();
            Sut.Reset(secret);
            var list = TestListOfStrings();
            var enumeratedList = Sut.Get(secret, list);
            Assert.IsNotNull(enumeratedList);
            var i = 0;
            foreach (var s in enumeratedList) {
                Assert.AreEqual(list[i], s);
                i ++;
            }
        }

        [TestMethod]
        public void CannotGetScriptSecretIfScriptCannotBeRunWithoutErrors() {
            var secret = new FailingSecretStringListEnumerator();
            Sut.Reset(secret);
            var list = TestListOfStrings();
            var enumeratedList = Sut.Get(secret, list);
            Assert.IsNull(enumeratedList);
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
    }
}