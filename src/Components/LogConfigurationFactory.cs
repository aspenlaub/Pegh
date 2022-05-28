using System;
using System.Diagnostics;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

// ReSharper disable once UnusedMember.Global
public class LogConfigurationFactory : ILogConfigurationFactory {
    private readonly string ApplicationName;
    private bool DetailedLogging = true;

    internal LogConfigurationFactory(string applicationName) {
        if (string.IsNullOrEmpty(applicationName)) {
            throw new ArgumentNullException(nameof(applicationName));
        }
        ApplicationName = applicationName;
    }

    public ILogConfiguration Create() {
        return new LogConfiguration {
            LogSubFolder = @"AspenlaubLogs\" + ApplicationName,
            LogId = $"{DateTime.Today:yyyy-MM-dd}-{Process.GetCurrentProcess().Id}",
            DetailedLogging = DetailedLogging
        };
    }

    public void SetDetailedLogging(bool detailedLogging) {
        DetailedLogging = detailedLogging;
    }
}