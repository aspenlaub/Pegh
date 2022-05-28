using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.SampleEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class XmlSchemerTest {
    protected const string CrewMemberSecretGuid = "E3C83BAF-AF26-0DCB-5C06-71CE6118C956";
    protected const string StarShipSecretGuid = "B5FCC87A-2EB2-2DFC-005C-ECBBB8EFF432";

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
        var errorsAndInfos = new ErrorsAndInfos();
        Assert.IsTrue(sut.Valid(CrewMemberSecretGuid, xml, typeof(CrewMember), errorsAndInfos));
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        Assert.IsFalse(sut.Valid(StarShipSecretGuid, xml, typeof(StarShip), errorsAndInfos));
        Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("The \'http://www.aspenlaub.net:CrewMember\' element is not declared")));
        var xsd = sut.Create(typeof(CrewMember)).Replace("firstname", "worstname");
        errorsAndInfos = new ErrorsAndInfos();
        Assert.IsFalse(XmlSchemer.Valid(CrewMemberSecretGuid, xml, xsd, errorsAndInfos));
        Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("The \'firstname\' attribute is not declared")));
    }
}