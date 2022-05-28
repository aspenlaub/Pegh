// ReSharper disable UnusedMember.Global
namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface ILogConfigurationFactory {
    ILogConfiguration Create();
    ILogConfigurationFactory CreateOtherFactory(string applicationName, bool detailedLogging);
}