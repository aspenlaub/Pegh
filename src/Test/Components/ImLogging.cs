using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

public class ImLogging : ImLoggingBase {
    public ImLogging(TimeSpan logEvery, DateTime endOfWork, ISimpleLogger simpleLogger, IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor)
        : base(logEvery, endOfWork, simpleLogger, methodNamesFromStackFramesExtractor) {
    }

    public async Task ImLoggingWorkAsync() {
        using (SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(ImLogging)))) {
            await WorkAsync();
        }
    }
}