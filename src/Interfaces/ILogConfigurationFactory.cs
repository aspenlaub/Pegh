namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface ILogConfigurationFactory {
    // ReSharper disable once UnusedMember.Global
    ILogConfiguration Create(string applicationName, bool detailedLogging);
}