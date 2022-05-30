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
    private readonly IList<string> StackOfScopes;

    private readonly ISimpleLogFlusher SimpleLogFlusher;
    private readonly IMethodNamesFromStackFramesExtractor MethodNamesFromStackFramesExtractor;

    private readonly IDictionary<string, string> ScopeToCreatorMethodMapping;

    public string LogSubFolder { get; set; } = @"AspenlaubLogs\Miscellaneous";

    public bool Enabled { get; }

    public SimpleLogger(ISimpleLogFlusher simpleLogFlusher, IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor) {
        LogEntries = new List<ISimpleLogEntry>();
        StackOfScopes = new List<string>();
        SimpleLogFlusher = simpleLogFlusher;
        MethodNamesFromStackFramesExtractor = methodNamesFromStackFramesExtractor;
        Enabled = true;
        ScopeToCreatorMethodMapping = new Dictionary<string, string>();
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
        if (!Enabled) { return; }

        if (StackOfScopes.Count == 0) {
            throw new Exception(Properties.Resources.AttemptToLogWithoutScope);
        }

        var reducesStackOfScopes = ReduceStackAccordingToCallStack(StackOfScopes);
        /*
        if (reducedStack.Count == 0) {
            throw new Exception(Properties.Resources.ScopeExistsButOutsideCallStack);
        }
        */

        lock (LockObject) {
            LogEntries.Add(SimpleLogEntry.Create(logLevel, reducesStackOfScopes.Count == 0 ? StackOfScopes : reducesStackOfScopes, formatter(state, exception)));
        }
        SimpleLogFlusher.Flush(this, LogSubFolder);
    }

    private IList<string> ReduceStackAccordingToCallStack(IList<string> stackOfScopes) {
        var callStackMethodNames = MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
        return stackOfScopes
               .Where(x => ScopeToCreatorMethodMapping.ContainsKey(x) && callStackMethodNames.Contains(ScopeToCreatorMethodMapping[x]))
               .ToList();
    }

    public bool IsEnabled(LogLevel logLevel) {
        return Enabled;
    }

    public IDisposable BeginScope<TState>(TState state) {
        if (state is not ISimpleLoggingScopeId loggingScope) {
            return new LoggingScope(() => { });
        }

        var scope = $"{loggingScope.Class}({loggingScope.Id})";
        lock (LockObject) {
            StackOfScopes.Add(scope);
            var callStackMethodNames = MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
            if (callStackMethodNames.Count < 2) {
                throw new Exception(Properties.Resources.CallStackTooSmallToFindCreatorMethodName);
            }
            ScopeToCreatorMethodMapping[scope] = callStackMethodNames[1];
        }
        return new LoggingScope(() => {
            lock (LockObject) {
                StackOfScopes.Remove(scope);
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