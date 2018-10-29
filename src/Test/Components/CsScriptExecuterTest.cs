using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    [TestClass]
    public class CsScriptExecuterTest {
        [TestMethod]
        public async Task CanExecuteCsScriptWithoutArguments() {
            ICsScriptExecuter sut = new CsScriptExecuter();
            var presetArgument = new List<ICsScriptArgument>();
            var csScript = new CsScript(new List<CsScriptArgument>(),  "1+1");
            var result = await sut.ExecuteCsScriptAsync(csScript, presetArgument, null);
            Assert.AreEqual("2", result);

            csScript = new CsScript(new List<CsScriptArgument>(), "\"1\"+\"1\"");
            result = await sut.ExecuteCsScriptAsync(csScript, presetArgument, null);
            Assert.AreEqual("11", result);
        }

        [TestMethod, ExpectedException(typeof(OperationCanceledException))]
        public async Task CannotRunInvalidCsScript() {
            ICsScriptExecuter sut = new CsScriptExecuter();
            var presetArgument = new List<ICsScriptArgument>();
            var csScript = new CsScript(new List<CsScriptArgument>(), "1+1+");
            await sut.ExecuteCsScriptAsync(csScript, presetArgument, null);
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public async Task CannotRunCsScriptThatThrowsAnException() {
            ICsScriptExecuter sut = new CsScriptExecuter();
            var presetArgument = new List<ICsScriptArgument>();
            var csScript = new CsScript(new List<CsScriptArgument>(), "throw new NotImplementedException();", "1+1");
            await sut.ExecuteCsScriptAsync(csScript, presetArgument, null);
        }
    }
}
