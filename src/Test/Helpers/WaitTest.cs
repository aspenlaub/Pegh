using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Helpers;

[TestClass]
public class WaitTest {
    [TestMethod]
    public void CanWait() {
        var now = DateTime.Now;
        Wait.Until(() => true, TimeSpan.FromSeconds(1));
        var elapsedMilliseconds = (int)DateTime.Now.Subtract(now).TotalMilliseconds;
        Assert.IsTrue(elapsedMilliseconds < 100, $"Expected less than 100 milliseconds elapsed time, it were {elapsedMilliseconds} milliseconds");

        var now2 = DateTime.Now;
        Wait.Until(() => DateTime.Now.Subtract(now2).TotalMilliseconds > 500, TimeSpan.FromSeconds(1));
        elapsedMilliseconds = (int)DateTime.Now.Subtract(now2).TotalMilliseconds;
        Assert.IsTrue(elapsedMilliseconds < 600, $"Expected less than 600 milliseconds elapsed time, it were {elapsedMilliseconds} milliseconds");

        var now3 = DateTime.Now;
        Wait.Until(() => DateTime.Now.Subtract(now3).TotalMilliseconds > 2000, TimeSpan.FromSeconds(1));
        elapsedMilliseconds = (int)DateTime.Now.Subtract(now3).TotalMilliseconds;
        Assert.IsTrue(elapsedMilliseconds < 1500, $"Expected less than 1500 milliseconds elapsed time, it were {elapsedMilliseconds} milliseconds");
    }

    [TestMethod]
    public async Task CanWaitAsync() {
        var now = DateTime.Now;
        await Wait.UntilAsync(async () => await Task.FromResult(true), TimeSpan.FromSeconds(1));
        var elapsedMilliseconds = (int)DateTime.Now.Subtract(now).TotalMilliseconds;
        Assert.IsTrue(elapsedMilliseconds < 100, $"Expected less than 100 milliseconds elapsed time, it were {elapsedMilliseconds} milliseconds");

        var now2 = DateTime.Now;
        await Wait.UntilAsync(async () => await Task.FromResult(DateTime.Now.Subtract(now2).TotalMilliseconds > 500), TimeSpan.FromSeconds(1));
        elapsedMilliseconds = (int)DateTime.Now.Subtract(now2).TotalMilliseconds;
        Assert.IsTrue(elapsedMilliseconds < 600, $"Expected less than 600 milliseconds elapsed time, it were {elapsedMilliseconds} milliseconds");

        var now3 = DateTime.Now;
        await Wait.UntilAsync(async () => await Task.FromResult(DateTime.Now.Subtract(now3).TotalMilliseconds > 2000), TimeSpan.FromSeconds(1));
        elapsedMilliseconds = (int)DateTime.Now.Subtract(now3).TotalMilliseconds;
        Assert.IsTrue(elapsedMilliseconds < 1500, $"Expected less than 1500 milliseconds elapsed time, it were {elapsedMilliseconds} milliseconds");
    }
}