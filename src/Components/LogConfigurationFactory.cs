using System;
using System.Diagnostics;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

// ReSharper disable once UnusedMember.Global
public class LogConfigurationFactory : ILogConfigurationFactory {
    private readonly string ApplicationName;
    private readonly bool DetailedLogging;

    public LogConfigurationFactory(string applicationName, bool detailedLogging) {
        ApplicationName = applicationName;
        DetailedLogging = detailedLogging;
    }

    public ILogConfiguration Create() {
        return new LogConfiguration {
            LogSubFolder = @"AspenlaubLogs\" + ApplicationName,
            LogId = $"{DateTime.Today:yyyy-MM-dd}-{Process.GetCurrentProcess().Id}",
            DetailedLogging = DetailedLogging
        };
    }

    public ILogConfigurationFactory CreateOtherFactory(string applicationName, bool detailedLogging) {
        return new LogConfigurationFactory(applicationName, detailedLogging);
    }
}