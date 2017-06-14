using System;
using System.IO;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.TestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class SecretRepositoryTest {
        private IComponentProvider ComponentProvider { get; set; }
        private ISecretRepository Sut { get; set; }
        private const string SomeFirstName = "Some First Name", SomeSurName = "Some Surname", SomeRank = "Some Rank";

        [TestInitialize]
        public void Initialize() {
            ComponentProvider = new ComponentProvider();
            Sut = ComponentProvider.SecretRepository;
            var folder = ComponentProvider.PeghEnvironment.RootWorkFolder + @"\SecretRepository";
            if (Directory.Exists(folder)) { return; }

            Directory.CreateDirectory(folder);
        }

        [TestMethod]
        public void DoesNotExistInRepositoryAfterRemoval() {
            var secret = new SecretCrewMember();
            Sut.Reset(secret);
            Assert.IsFalse(Sut.Exists(secret));
        }

        [TestMethod]
        public void CanGetDefault() {
            var secret = new SecretCrewMember { Value = new CrewMember { FirstName = SomeFirstName } };
            Sut.Reset(secret);
            Assert.AreEqual(SomeFirstName, secret.Value.FirstName);
            Sut.Get(secret);
            Assert.AreEqual(SecretCrewMember.DefaultFirstName, secret.Value.FirstName);
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
            secret.Value = new CrewMember { FirstName = SomeFirstName, SurName = SomeSurName, Rank = SomeRank };
            Sut.Set(secret);
            secret.Value = new CrewMember();
            Assert.IsNull(secret.Value.FirstName);
            Assert.IsNull(secret.Value.SurName);
            Assert.IsNull(secret.Value.Rank);
            Sut.Get(secret);
            Assert.AreEqual(SomeFirstName, secret.Value.FirstName);
            Assert.AreEqual(SomeSurName, secret.Value.SurName);
            Assert.AreEqual(SomeRank, secret.Value.Rank);
        }

        [TestMethod]
        public void FixedGetWithNonFixedSecretReturnsDefault() {
            var secret = new SecretCrewMember { Value = new CrewMember { FirstName = SomeFirstName }};
            Sut.Reset(secret);
            secret.SecretType = SecretTypes.Script;
            Assert.AreEqual(SomeFirstName, secret.Value.FirstName);
            Sut.Get(secret);
            Assert.AreEqual(SecretCrewMember.DefaultFirstName, secret.Value.FirstName);
        }

        [TestMethod]
        public void ScriptGetWithFixedSecretReturnsDefault() {
            var secret = new SecretCrewMember { Value = new CrewMember { FirstName = SomeFirstName } };
            Sut.Reset(secret);
            Sut.Get(secret, SomeFirstName);
            Assert.AreEqual(SecretCrewMember.DefaultFirstName, secret.Value.FirstName);
        }

        [TestMethod, ExpectedException(typeof(NotImplementedException))]
        public void ScriptGetWithScriptSecretIsNotImplementedYet() {
            var secret = new SecretCrewMember { Value = new CrewMember { FirstName = SomeFirstName }, SecretType = SecretTypes.Script };
            Sut.Reset(secret);
            Sut.Get(secret, SomeFirstName);
        }
    }
}