using System;
using System.Collections.Generic;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class SimpleLogger : ISimpleLogger {
    private const int MaxLogEntries = 10000;

    private static readonly object LockObject = new();

    private readonly List<ISimpleLogEntry> LogEntries;
    private readonly IList<string> Stack;
    private readonly ISimpleLogFlusher SimpleLogFlusher;

    public string LogSubFolder { get; set; } = @"AspenlaubLogs\Miscellaneous";

    public bool Enabled { get; }

    public SimpleLogger(ISimpleLogFlusher simpleLogFlusher) {
        LogEntries = new List<ISimpleLogEntry>();
        Stack = new List<string>();
        SimpleLogFlusher = simpleLogFlusher;
        Enabled = true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
        if (!Enabled) { return; }

        lock (LockObject) {
            LogEntries.Add(SimpleLogEntry.Create(logLevel, Stack, formatter(state, exception)));
        }
        SimpleLogFlusher.Flush(this, LogSubFolder);
    }

    public bool IsEnabled(LogLevel logLevel) {
        return Enabled;
    }

    public IDisposable BeginScope<TState>(TState state) {
        if (state is not ISimpleLoggingScopeId loggingScope) {
            return new LoggingScope(() => { });
        }

        var stackEntry = $"{loggingScope.Class}({loggingScope.Id})";
        lock (LockObject) {
            Stack.Add(stackEntry);
        }
        return new LoggingScope(() => {
            lock (LockObject) {
                Stack.Remove(stackEntry);
            }
        });
    }

    public IList<ISimpleLogEntry> FindLogEntries(Func<ISimpleLogEntry, bool> condition) {
        lock (LockObject) {
            var logEntries = new List<ISimpleLogEntry>();
            logEntries.AddRange(LogEntries.Where(condition));
            return logEntries;
        }
    }

    public void OnEntriesFlushed(IList<ISimpleLogEntry> entries) {
        lock (LockObject) {
            foreach (var entry in entries) {
                entry.Flushed = true;
            }

            if (LogEntries.Count < MaxLogEntries) { return; }

            int i;
            for (i = 0; LogEntries[i].Flushed && i < MaxLogEntries / 2; i++) {
            }
            LogEntries.RemoveRange(0, i);
        }
    }
}