using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class MethodNamesFromStackFramesExtractorTest {
    private readonly MethodNamesFromStackFramesExtractor Sut = new();

    [TestMethod]
    public void MethodNamesFromStackFramesExtractor_WithMethodCall_ReturnsCallerAndCallee() {
        var methodNames = MethodReturningMethodNamesFromStackFramesWhenCalled();
        Assert.IsTrue(methodNames.Contains(nameof(MethodReturningMethodNamesFromStackFramesWhenCalled)));
        Assert.IsTrue(methodNames.Contains(nameof(MethodNamesFromStackFramesExtractor_WithMethodCall_ReturnsCallerAndCallee)));
        Assert.AreEqual(2, methodNames.Count);
    }

    [TestMethod]
    public async Task MethodNamesFromStackFramesExtractor_WithAsyncMethodCall_ReturnsCallerAndCallee() {
        var methodNames = await MethodReturningMethodNamesFromStackFramesWhenCalledAsync();
        Assert.IsTrue(methodNames.Contains(nameof(MethodReturningMethodNamesFromStackFramesWhenCalledAsync)));
        Assert.IsTrue(methodNames.Contains(nameof(MethodNamesFromStackFramesExtractor_WithAsyncMethodCall_ReturnsCallerAndCallee)));
        Assert.AreEqual(2, methodNames.Count);

        await Task.WhenAll(new List<Task> {
            Task.Run(async () => methodNames = await MethodReturningMethodNamesFromStackFramesWhenCalledAsync())
        });
        Assert.IsTrue(methodNames.Contains(nameof(MethodReturningMethodNamesFromStackFramesWhenCalledAsync)));
        Assert.IsTrue(methodNames.Contains(nameof(MethodNamesFromStackFramesExtractor_WithAsyncMethodCall_ReturnsCallerAndCallee)));
        Assert.AreEqual(2, methodNames.Count);
    }

    protected IList<string> MethodReturningMethodNamesFromStackFramesWhenCalled() {
        return Sut.ExtractMethodNamesFromStackFrames();
    }

    protected async Task<IList<string>> MethodReturningMethodNamesFromStackFramesWhenCalledAsync() {
        await Task.CompletedTask;
        return Sut.ExtractMethodNamesFromStackFrames();
    }

}