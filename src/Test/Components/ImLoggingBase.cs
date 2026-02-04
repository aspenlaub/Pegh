using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

public class ImLoggingBase {
    protected readonly TimeSpan LogEvery;
    protected readonly DateTime EndOfWork;
    protected readonly ISimpleLogger SimpleLogger;
    protected readonly string WorkerId;
    protected readonly IMethodNamesFromStackFramesExtractor MethodNamesFromStackFramesExtractor;

    protected ImLoggingBase(TimeSpan logEvery, DateTime endOfWork, ISimpleLogger simpleLogger, IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor) {
        LogEvery = logEvery;
        EndOfWork = endOfWork;
        SimpleLogger = simpleLogger;
        MethodNamesFromStackFramesExtractor = methodNamesFromStackFramesExtractor;
        WorkerId = Guid.NewGuid().ToString().Substring(0, 5);
    }

    protected async Task WorkAsync() {
        IList<string> methodNamesFromStack = MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
        Assert.Contains(nameof(WorkAsync), methodNamesFromStack);
        SimpleLogger.LogWarningWithCallStack($"{WorkerId} is working hard", methodNamesFromStack);
        SimpleLogger.LogInformationWithCallStack($"{WorkerId} starts working", methodNamesFromStack);
        while (DateTime.Now < EndOfWork) {
            SimpleLogger.LogInformationWithCallStack($"{WorkerId} does something", methodNamesFromStack);
            await Task.Delay(LogEvery);
        }
        SimpleLogger.LogErrorWithCallStack($"{WorkerId} wants to work more", methodNamesFromStack);
    }
}