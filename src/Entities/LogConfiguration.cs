using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class LogConfiguration : ILogConfiguration {
    public string LogSubFolder { get; init; }
    public string LogId { get; init; }
    public bool DetailedLogging { get; init; }
}