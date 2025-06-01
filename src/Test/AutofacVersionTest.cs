using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test;

[TestClass]
public class AutofacVersionTest {
    private const string _expectedVersion = "7.0.1.0";

    [TestMethod]
    public void PeghUsesRightAutofacPackageVersion() {
        Version version = typeof(Autofac.ContainerBuilder).Assembly.GetName().Version;
        Assert.IsNotNull(version);
        Assert.IsTrue(version.ToString().StartsWith(_expectedVersion), $"Expected version {_expectedVersion} is needed for Shatilaya to work, cannot use {version}");
    }
}