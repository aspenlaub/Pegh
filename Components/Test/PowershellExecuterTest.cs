using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class PowershellExecuterTest {
        [TestMethod]
        public void CanRunPowershellScript() {
            var sut = new PowershellExecuter();
            var fileName = Path.GetTempPath() + @"\helloworld.ps1";
            File.WriteAllText(fileName, "$s = \"Hello World\"");
            IList<string> errors;
            sut.ExecutePowershellScriptFile(fileName, out errors);
            Assert.IsTrue(!errors.Any());
        }

        [TestMethod]
        public void CannotRunInvalidPowershellScript() {
            var sut = new PowershellExecuter();
            var fileName = Path.GetTempPath() + @"\helloworld.ps1";
            File.WriteAllText(fileName, "$s = Hello World\"");
            IList<string> errors;
            sut.ExecutePowershellScriptFile(fileName, out errors);
            Assert.IsTrue(errors.Any());
        }
    }
}
