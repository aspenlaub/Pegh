using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    [TestClass]
    public class PlatformTest {
        [TestMethod]
        public void CanGetOperatingSystem() {
            IPlatform sut = new Platform();
            var operatingSystem = sut.OperatingSystem();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(operatingSystem));
        }
    }
}
