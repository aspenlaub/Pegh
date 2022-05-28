using System;
using System.Diagnostics;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

// ReSharper disable once UnusedMember.Global
public class LogConfigurationFactory : ILogConfigurationFactory {
    private string ApplicationName = "";
    private bool DetailedLogging = true;

    public ILogConfiguration Create() {
        if (string.IsNullOrEmpty(ApplicationName)) {
            throw new Exception($"{nameof(LogConfigurationFactory)} was not initialized");
        }
        return new LogConfiguration {
            LogSubFolder = @"AspenlaubLogs\" + ApplicationName,
            LogId = $"{DateTime.Today:yyyy-MM-dd}-{Process.GetCurrentProcess().Id}",
            DetailedLogging = DetailedLogging
        };
    }

    public void InitializeIfNecessary(string applicationName, bool detailedLogging) {
        if (!string.IsNullOrEmpty(ApplicationName)) { return; }

        ApplicationName = applicationName;
        DetailedLogging = detailedLogging;
    }
}