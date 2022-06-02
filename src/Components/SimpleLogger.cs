using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class SimpleLogger : ISimpleLogger {
    private const int MaxLogEntries = 10000;

    private static readonly object LockObject = new();

    private readonly List<ISimpleLogEntry> _LogEntries;
    private readonly IList<string> _StackOfScopes;

    private readonly ISimpleLogFlusher _SimpleLogFlusher;
    private readonly IMethodNamesFromStackFramesExtractor _MethodNamesFromStackFramesExtractor;

    private readonly IDictionary<string, string> _ScopeToCreatorMethodMapping;

    public string LogSubFolder { get; }
    public string LogId { get; }

    public bool Enabled { get; }

    public SimpleLogger(ILogConfiguration logConfiguration, ISimpleLogFlusher simpleLogFlusher, IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor) {
        LogSubFolder = logConfiguration.LogSubFolder;
        LogId = logConfiguration.LogId;
        _LogEntries = new List<ISimpleLogEntry>();
        _StackOfScopes = new List<string>();
        _SimpleLogFlusher = simpleLogFlusher;
        _MethodNamesFromStackFramesExtractor = methodNamesFromStackFramesExtractor;
        Enabled = true;
        _ScopeToCreatorMethodMapping = new Dictionary<string, string>();
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
        if (!Enabled) { return; }

        if (_StackOfScopes.Count == 0) {
            throw new Exception(Properties.Resources.AttemptToLogWithoutScope);
        }

        const string noMessage = "(empty)";
        LogMessageWithCallStack logMessageWithCallStack;
        var message = state.ToString() ?? noMessage;
        try {
            if (message == noMessage) {
                throw new JsonException($"Could not deserialize {message}");
            }
            logMessageWithCallStack = JsonSerializer.Deserialize<LogMessageWithCallStack>(message);
            if (logMessageWithCallStack == null) {
                throw new JsonException($"Could not deserialize {message}");
            }
        } catch {
            throw new JsonException($"Could not deserialize {message}");
        }

        var reducesStackOfScopes = ReduceStackAccordingToCallStack(_StackOfScopes, logMessageWithCallStack.MethodNamesInCallStack);
        if (reducesStackOfScopes.Count == 0) {
            throw new Exception(Properties.Resources.ScopeExistsButOutsideCallStack);
        }

        lock (LockObject) {
            _LogEntries.Add(SimpleLogEntry.Create(logLevel, reducesStackOfScopes.Count == 0 ? _StackOfScopes : reducesStackOfScopes, logMessageWithCallStack.Message));
        }
        _SimpleLogFlusher.Flush(this, LogSubFolder);
    }

    private IList<string> ReduceStackAccordingToCallStack(IEnumerable<string> stackOfScopes, IEnumerable<string> methodNamesFromCallStack) {
        return stackOfScopes
               .Where(x => _ScopeToCreatorMethodMapping.ContainsKey(x) && methodNamesFromCallStack.Contains(_ScopeToCreatorMethodMapping[x]))
               .ToList();
    }

    public bool IsEnabled(LogLevel logLevel) {
        return Enabled;
    }

    public IDisposable BeginScope<TState>(TState state) {
        if (state is not ISimpleLoggingScopeId loggingScope) {
            return new LoggingScope(() => { });
        }

        var scope = $"{loggingScope.ClassOrMethod}({loggingScope.Id})";
        lock (LockObject) {
            if (_StackOfScopes.Contains(scope)) {
                throw new Exception(string.Format(Properties.Resources.ScopeAlreadyBegan, scope));
            }
            _StackOfScopes.Add(scope);
            var callStackMethodNames = _MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
            if (callStackMethodNames.Count < 2) {
                throw new Exception(Properties.Resources.CallStackTooSmallToFindCreatorMethodName);
            }
            _ScopeToCreatorMethodMapping[scope] = callStackMethodNames[1];
        }
        return new LoggingScope(() => OnLoggingScopeDisposing(scope));
    }

    private void OnLoggingScopeDisposing(string scope) {
       lock (LockObject) {
           _StackOfScopes.Remove(scope);
       }
    }

    public IList<ISimpleLogEntry> FindLogEntries(Func<ISimpleLogEntry, bool> condition) {
        lock (LockObject) {
            var logEntries = new List<ISimpleLogEntry>();
            logEntries.AddRange(_LogEntries.Where(condition));
            return logEntries;
        }
    }

    public void OnEntriesFlushed(IList<ISimpleLogEntry> entries) {
        lock (LockObject) {
            foreach (var entry in entries) {
                entry.Flushed = true;
            }

            if (_LogEntries.Count < MaxLogEntries) { return; }

            int i;
            for (i = 0; _LogEntries[i].Flushed && i < MaxLogEntries / 2; i++) {
            }
            _LogEntries.RemoveRange(0, i);
        }
    }
}