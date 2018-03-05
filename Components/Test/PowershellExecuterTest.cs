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

        [TestMethod]
        public void CannotRunPowershellScriptThatWritesErrors() {
            var sut = new PowershellExecuter();
            var fileName = Path.GetTempPath() + @"\helloworld.ps1";
            File.WriteAllText(fileName, "Write-Error(\"Hello World\")");
            IList<string> errors;
            sut.ExecutePowershellScriptFile(fileName, out errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Hello World", errors[0]);
        }

        [TestMethod]
        public void PowershellScriptIsRunInItsFolder() {
            var sut = new PowershellExecuter();
            var fileName = Path.GetTempPath() + @"\helloworld.ps1";
            var script = new List<string>();
            script.Add("$path = (Resolve-Path .\\).Path + \"\\\"");
            script.Add("if ($path -eq \"" + Path.GetTempPath() + "\") { Write-Output \"Path OK\"} else { Write-Error (\"Path Not OK \" + $path) }");
            script.Add("$path = (Split-Path $MyInvocation.MyCommand.Path -Parent) + \"\\\"");
            script.Add("if ($path -eq \"" + Path.GetTempPath() + "\") { Write-Output \"CommandPath OK\"} else { Write-Error (\"CommandPath Not OK \" + $path) }");
            File.WriteAllText(fileName, string.Join("\r\n", script));
            IList<string> errors;
            sut.ExecutePowershellScriptFile(fileName, out errors);
            Assert.IsTrue(!errors.Any());
        }
    }
}
