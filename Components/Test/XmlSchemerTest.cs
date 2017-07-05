using Aspenlaub.Net.GitHub.CSharp.Pegh.TestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class XmlSchemerTest {
        [TestMethod]
        public void CanCreateXmlSchema() {
            var sut = new XmlSchemer();
            var xsd = sut.Create(typeof(StarFleet));
            Assert.IsTrue(xsd.Length > 100);
            Assert.IsTrue(xsd.Contains("utf-8"));
            Assert.IsTrue(xsd.Contains(nameof(CrewMember)));
        }

        [TestMethod]
        public void CanValidateAgainstXmlSchema() {
            var sut = new XmlSchemer();
            var serializer = new XmlSerializer();
            var crewMember = new CrewMember { FirstName = "B'Elanna", SurName = "Torres", Rank = "Lieutenant" };
            var xml = serializer.Serialize(crewMember);
            Assert.IsTrue(sut.Valid(xml, typeof(CrewMember)));
            Assert.IsFalse(sut.Valid(xml, typeof(StarShip)));
            var xsd = sut.Create(typeof(CrewMember)).Replace("firstname", "worstname");
            Assert.IsFalse(sut.Valid(xml, xsd, typeof(CrewMember)));
        }
    }
}
