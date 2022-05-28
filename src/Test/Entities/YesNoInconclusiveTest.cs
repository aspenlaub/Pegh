using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities;

[TestClass]
public class YesNoInconclusiveTest {
    [TestMethod]
    public void CanFormatYesNoInconclusive() {
        Assert.AreEqual("Inconclusive", new YesNoInconclusive { Inconclusive = true, YesNo = true }.ToString());
        Assert.AreEqual("Inconclusive", new YesNoInconclusive { Inconclusive = true, YesNo = false }.ToString());
        Assert.AreEqual("Yes", new YesNoInconclusive { Inconclusive = false, YesNo = true }.ToString());
        Assert.AreEqual("No", new YesNoInconclusive { Inconclusive = false, YesNo = false }.ToString());
    }
}