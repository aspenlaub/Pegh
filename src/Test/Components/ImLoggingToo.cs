using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

public class ImLoggingToo : ImLoggingBase {
    public ImLoggingToo(TimeSpan logEvery, DateTime endOfWork, ISimpleLogger simpleLogger, IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor)
        : base(logEvery, endOfWork, simpleLogger, methodNamesFromStackFramesExtractor) {
    }

    public async Task ImLoggingWorkTooAsync() {
        using (SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(ImLogging), WorkerId))) {
            await WorkAsync();
        }
    }
}