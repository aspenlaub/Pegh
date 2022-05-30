using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    public class ImLogging {
        private readonly TimeSpan LogEvery;
        private readonly DateTime EndOfWork;
        private readonly ISimpleLogger SimpleLogger;
        private readonly string WorkerId;

        public ImLogging(TimeSpan logEvery, DateTime endOfWork, ISimpleLogger simpleLogger) {
            LogEvery = logEvery;
            EndOfWork = endOfWork;
            SimpleLogger = simpleLogger;
            WorkerId = Guid.NewGuid().ToString().Substring(0, 5);

        }

        public async Task Work() {
            using (SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(ImLogging), WorkerId))) {
                SimpleLogger.LogInformation($"{WorkerId} starts working");
                while (DateTime.Now < EndOfWork) {
                    SimpleLogger.LogInformation($"{WorkerId} does something");
                    await Task.Delay(LogEvery);
                }
            }
        }
    }
}
