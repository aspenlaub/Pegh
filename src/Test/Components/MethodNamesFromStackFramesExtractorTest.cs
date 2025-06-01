using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class MethodNamesFromStackFramesExtractorTest {
    private readonly MethodNamesFromStackFramesExtractor _Sut = new();

    [TestMethod]
    public void ExtractMethodNamesFromStackFrames_WithinMethodCall_ReturnsCallerAndCallee() {
        var methodNames = MethodReturningMethodNamesFromStackFramesWhenCalled().ToList();
        RemoveTestFrameworkMethodNames(methodNames);
        Assert.IsTrue(methodNames.Contains(nameof(MethodReturningMethodNamesFromStackFramesWhenCalled)));
        Assert.IsTrue(methodNames.Contains(nameof(ExtractMethodNamesFromStackFrames_WithinMethodCall_ReturnsCallerAndCallee)));
        Assert.AreEqual(3, methodNames.Count, string.Join('/', methodNames));
    }

    [TestMethod]
    public async Task ExtractMethodNamesFromStackFrames_WithinAsyncMethodCall_ReturnsCallerAndCallee() {
        var methodNames = (await MethodReturningMethodNamesFromStackFramesWhenCalledAsync()).ToList();
        RemoveTestFrameworkMethodNames(methodNames);
        Assert.IsTrue(methodNames.Contains(nameof(MethodReturningMethodNamesFromStackFramesWhenCalledAsync)));
        Assert.IsTrue(methodNames.Contains(nameof(ExtractMethodNamesFromStackFrames_WithinAsyncMethodCall_ReturnsCallerAndCallee)));
        Assert.AreEqual(4, methodNames.Count, string.Join('/', methodNames));

        await Task.WhenAll(new List<Task> {
            Task.Run(async () => methodNames = (await MethodReturningMethodNamesFromStackFramesWhenCalledAsync()).ToList())
        });
        Assert.IsTrue(methodNames.Contains(nameof(MethodReturningMethodNamesFromStackFramesWhenCalledAsync)));
        Assert.IsTrue(methodNames.Contains(nameof(ExtractMethodNamesFromStackFrames_WithinAsyncMethodCall_ReturnsCallerAndCallee)));
        Assert.AreEqual(2, methodNames.Count);
    }

    private readonly string[] _TestFrameworkMethodNames = [
        "RunTestsAsync", "RunTestsFromRightContextAsync", "ExecuteTestsAsync",
        "ExecuteTestsInSourceAsync", "RunSingleTestAsync", "ExecuteTestsWithTestRunnerAsync"
    ];

    private void RemoveTestFrameworkMethodNames(IList<string> methodNames) {
        if (!methodNames.Contains("RunTestsAsync")) { return; }

        for (int i = methodNames.Count - 1; i > 0 && _TestFrameworkMethodNames.Contains(methodNames[i]); i--) {
            methodNames.RemoveAt(i);
        }
    }

    protected IEnumerable<string> MethodReturningMethodNamesFromStackFramesWhenCalled() {
        return _Sut.ExtractMethodNamesFromStackFrames();
    }

    protected async Task<IEnumerable<string>> MethodReturningMethodNamesFromStackFramesWhenCalledAsync() {
        await Task.CompletedTask;
        return _Sut.ExtractMethodNamesFromStackFrames();
    }

}