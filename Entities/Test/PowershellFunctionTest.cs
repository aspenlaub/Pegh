using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities.Test {
    [TestClass]
    public class PowershellFunctionTest {
        [TestMethod]
        public void CanWriteAndReadScript() {
            const string script = "This is not a script, just a proof of concept";
            var sut = new PowershellFunction<string, string> {
                Script = script
            };
            var serializedScript = sut.SerializedScript;
            sut.Script = "";
            sut.SerializedScript = serializedScript;
            Assert.AreEqual(script, sut.Script);
        }
    }
}
