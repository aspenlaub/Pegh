using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class PowershellExecuterTest {
        [TestMethod]
        public void CanRunPowershellScript() {
            var sut = new PowershellExecuter();
            Assert.IsTrue(sut.ExecutePowershellFunction("$s = \"Hello World\""));
        }

        [TestMethod]
        public void CannotRunInvalidPowershellScript() {
            var sut = new PowershellExecuter();
            Assert.IsFalse(sut.ExecutePowershellFunction("$s = Hello World\""));
        }
    }
}
