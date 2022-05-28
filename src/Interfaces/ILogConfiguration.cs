// ReSharper disable UnusedMemberInSuper.Global
namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface ILogConfiguration {
    string LogSubFolder { get; init; }
    string LogId { get; init; }
    bool DetailedLogging { get; init; }
}