using System;
using System.Diagnostics;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class LogConfiguration : ILogConfiguration {
    public static ILogConfiguration Instance { get; private set; }

    public LogConfiguration(string applicationName) {
        LogSubFolder = @"AspenlaubLogs\" + applicationName;
        Instance = this;
    }

    public string LogSubFolder { get; init; }
    public string LogId { get; } = $"{DateTime.Today:yyyy-MM-dd}-{Process.GetCurrentProcess().Id}";
    public bool DetailedLogging { get; set; } = true;
}