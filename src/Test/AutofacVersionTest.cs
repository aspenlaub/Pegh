using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test;

[TestClass]
public class AutofacVersionTest {
    private const string ExpectedVersion = "7.0.1.0";

    [TestMethod]
    public void PeghUsesRightAutofacPackageVersion() {
        var version = typeof(Autofac.ContainerBuilder).Assembly.GetName().Version;
        Assert.IsNotNull(version);
        Assert.IsTrue(version.ToString().StartsWith(ExpectedVersion), $"Expected version {ExpectedVersion} is needed for Shatilaya to work, cannot use {version}");
    }
}