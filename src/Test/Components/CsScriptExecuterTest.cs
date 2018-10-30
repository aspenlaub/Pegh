using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    [TestClass]
    public class CsScriptExecuterTest {
        [TestMethod]
        public async Task CanExecuteCsScriptWithoutArguments() {
            var componentProviderMock = new Mock<IComponentProvider>();
            componentProviderMock.SetupGet(c => c.CsScriptMarshaller).Returns(new CsScriptMarshaller());
            ICsScriptExecuter sut = new CsScriptExecuter(componentProviderMock.Object);
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
            var componentProviderMock = new Mock<IComponentProvider>();
            componentProviderMock.SetupGet(c => c.CsScriptMarshaller).Returns(new CsScriptMarshaller());
            ICsScriptExecuter sut = new CsScriptExecuter(componentProviderMock.Object);
            var presetArgument = new List<ICsScriptArgument>();
            var csScript = new CsScript(new List<CsScriptArgument>(), "1+1+");
            await sut.ExecuteCsScriptAsync(csScript, presetArgument, null);
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public async Task CannotRunCsScriptThatThrowsAnException() {
            var componentProviderMock = new Mock<IComponentProvider>();
            componentProviderMock.SetupGet(c => c.CsScriptMarshaller).Returns(new CsScriptMarshaller());
            ICsScriptExecuter sut = new CsScriptExecuter(componentProviderMock.Object);
            var presetArgument = new List<ICsScriptArgument>();
            var csScript = new CsScript(new List<CsScriptArgument>(), "throw new NotImplementedException();", "1+1");
            await sut.ExecuteCsScriptAsync(csScript, presetArgument, null);
        }
    }
}
