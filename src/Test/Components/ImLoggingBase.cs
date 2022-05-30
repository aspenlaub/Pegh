using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    public class ImLoggingBase {
        protected readonly TimeSpan LogEvery;
        protected readonly DateTime EndOfWork;
        protected readonly ISimpleLogger SimpleLogger;
        protected readonly string WorkerId;

        protected ImLoggingBase(TimeSpan logEvery, DateTime endOfWork, ISimpleLogger simpleLogger) {
            LogEvery = logEvery;
            EndOfWork = endOfWork;
            SimpleLogger = simpleLogger;
            WorkerId = Guid.NewGuid().ToString().Substring(0, 5);

        }

        protected async Task WorkAsync() {
            SimpleLogger.LogInformation($"{WorkerId} starts working");
            while (DateTime.Now < EndOfWork) {
                SimpleLogger.LogInformation($"{WorkerId} does something");
                await Task.Delay(LogEvery);
            }
        }
    }
}
