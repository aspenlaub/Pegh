using System;
using System.Diagnostics;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

// ReSharper disable once UnusedMember.Global
public class LogConfigurationFactory : ILogConfigurationFactory {
    public ILogConfiguration Create(string applicationName, bool detailedLogging) {
        return new LogConfiguration {
            LogSubFolder = @"AspenlaubLogs\" + applicationName,
            LogId = $"{DateTime.Today:yyyy-MM-dd}-{Process.GetCurrentProcess().Id}",
            DetailedLogging = detailedLogging
        };
    }
}