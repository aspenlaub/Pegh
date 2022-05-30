using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities;

[TestClass]
public class SimpleLoggerTest {
    [TestMethod]
    public void CanSetLogSubFolder() {
        ISimpleLogger sut = new SimpleLogger(new SimpleLogFlusher(), new MethodNamesFromStackFramesExtractor());
        sut.LogSubFolder = @"AspenlaubLogs\" + nameof(SimpleLoggerTest);
    }
}