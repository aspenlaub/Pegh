using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    [TestClass]
    public class PowershellExecuterTest {
        [TestMethod]
        public void CanRunPowershellScript() {
            var sut = new PowershellExecuter();
            var fileName = Path.GetTempPath() + @"\helloworld.ps1";
            File.WriteAllText(fileName, @"$s = ""Hello World""");
            IList<string> errors;
            sut.ExecutePowershellScriptFile(fileName, out errors);
            Assert.IsTrue(!errors.Any());
        }

        [TestMethod]
        public void CannotRunInvalidPowershellScript() {
            var sut = new PowershellExecuter();
            var fileName = Path.GetTempPath() + @"\helloworld.ps1";
            File.WriteAllText(fileName, @"$s = Hello World""");
            IList<string> errors;
            sut.ExecutePowershellScriptFile(fileName, out errors);
            Assert.IsTrue(errors.Any());
        }

        [TestMethod]
        public void CannotRunPowershellScriptThatWritesErrors() {
            var sut = new PowershellExecuter();
            var fileName = Path.GetTempPath() + @"\helloworld.ps1";
            File.WriteAllText(fileName, @"Write-Error(""Hello World"")");
            IList<string> errors;
            sut.ExecutePowershellScriptFile(fileName, out errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Hello World", errors[0]);
        }

        [TestMethod]
        public void PowershellScriptIsRunInItsFolder() {
            var sut = new PowershellExecuter();
            var fileName = Path.GetTempPath() + @"\helloworld.ps1";
            var script = new List<string> {
                "$path = (Resolve-Path .\\).Path + \"\\\"",
                "if ($path -eq \"" + Path.GetTempPath() +
                "\") { Write-Output \"Path OK\"} else { Write-Error (\"Path Not OK \" + $path) }",
                "$path = (Split-Path $MyInvocation.MyCommand.Path -Parent) + \"\\\"",
                "if ($path -eq \"" + Path.GetTempPath() +
                "\") { Write-Output \"CommandPath OK\"} else { Write-Error (\"CommandPath Not OK \" + $path) }"
            };
            File.WriteAllText(fileName, string.Join("\r\n", script));
            IList<string> errors;
            sut.ExecutePowershellScriptFile(fileName, out errors);
            Assert.IsTrue(!errors.Any());
        }
    }
}
