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
        Assert.Contains(nameof(MethodReturningMethodNamesFromStackFramesWhenCalled), methodNames);
        Assert.Contains(nameof(ExtractMethodNamesFromStackFrames_WithinMethodCall_ReturnsCallerAndCallee), methodNames);
        Assert.IsLessThanOrEqualTo(4, methodNames.Count, string.Join('/', methodNames));
    }

    [TestMethod]
    public async Task ExtractMethodNamesFromStackFrames_WithinAsyncMethodCall_ReturnsCallerAndCallee() {
        var methodNames = (await MethodReturningMethodNamesFromStackFramesWhenCalledAsync()).ToList();
        Assert.Contains(nameof(MethodReturningMethodNamesFromStackFramesWhenCalledAsync), methodNames);
        Assert.Contains(nameof(ExtractMethodNamesFromStackFrames_WithinAsyncMethodCall_ReturnsCallerAndCallee), methodNames);
        Assert.IsLessThanOrEqualTo(4, methodNames.Count, string.Join('/', methodNames));

        await Task.WhenAll(new List<Task> {
            Task.Run(async () => methodNames = (await MethodReturningMethodNamesFromStackFramesWhenCalledAsync()).ToList())
        });
        Assert.Contains(nameof(MethodReturningMethodNamesFromStackFramesWhenCalledAsync), methodNames);
        Assert.Contains(nameof(ExtractMethodNamesFromStackFrames_WithinAsyncMethodCall_ReturnsCallerAndCallee), methodNames);
        Assert.HasCount(2, methodNames);
    }

    protected IEnumerable<string> MethodReturningMethodNamesFromStackFramesWhenCalled() {
        return _Sut.ExtractMethodNamesFromStackFrames();
    }

    protected async Task<IEnumerable<string>> MethodReturningMethodNamesFromStackFramesWhenCalledAsync() {
        await Task.CompletedTask;
        return _Sut.ExtractMethodNamesFromStackFrames();
    }

}