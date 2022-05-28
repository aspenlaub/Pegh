// ReSharper disable UnusedMember.Global
namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface ILogConfigurationFactory {
    ILogConfiguration Create();
    void SetDetailedLogging(bool detailedLogging);
}