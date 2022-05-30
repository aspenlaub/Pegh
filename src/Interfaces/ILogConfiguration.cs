// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global
namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface ILogConfiguration {
    string LogSubFolder { get; }
    string LogId { get; }
    bool DetailedLogging { get; set; }

    static ILogConfiguration Instance => null;
}