using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Helpers;

public static class Wait {
    public static void Until(Func<bool> condition, TimeSpan timeSpan) {
        var milliseconds = timeSpan.Milliseconds + 1000 * timeSpan.TotalSeconds;
        var internalMilliseconds = (int)Math.Ceiling(1 + milliseconds / 20);
        do {
            if (condition()) { return; }

            Thread.Sleep(internalMilliseconds); // Do not use await Task.Delay here
            milliseconds -= internalMilliseconds;
        } while (milliseconds >= 0);

    }

    public static async Task UntilAsync(Func<Task<bool>> condition, TimeSpan timeSpan) {
        var milliseconds = timeSpan.Milliseconds + 1000 * timeSpan.TotalSeconds;
        var internalMilliseconds = (int)Math.Ceiling(1 + milliseconds / 20);
        do {
            if (await condition()) { return; }

            Thread.Sleep(internalMilliseconds); // Do not use await Task.Delay here
            milliseconds -= internalMilliseconds;
        } while (milliseconds >= 0);

    }
}