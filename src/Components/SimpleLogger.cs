using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class SimpleLogger(ILogConfiguration logConfiguration, ISimpleLogFlusher simpleLogFlusher,
            IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor,
            IExceptionFolderProvider exceptionFolderProvider) : ISimpleLogger {
    private const int MaxLogEntries = 10000;
    private const string CouldNotBeDetermined = nameof(CouldNotBeDetermined);

    private static readonly object LockObject = new();

    private readonly List<ISimpleLogEntry> _LogEntries = new();
    private readonly IList<string> _StackOfScopes = new List<string>();

    private readonly IDictionary<string, string> _ScopeToCreatorMethodMapping = new Dictionary<string, string>();

    public string LogSubFolder { get; } = logConfiguration.LogSubFolder;
    public string LogId { get; } = logConfiguration.LogId;

    public bool Enabled => true;

    private readonly IFolder _ExceptionFolder = exceptionFolderProvider.ExceptionFolder();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
        simpleLogFlusher.FlushIsRequired = true;

        if (!Enabled) { return; }

        if (_StackOfScopes.Count == 0) {
            WriteErrorToExceptionFolder(Properties.Resources.AttemptToLogWithoutScope);
            return;
        }

        const string noMessage = "(empty)";
        LogMessageWithCallStack logMessageWithCallStack;
        var message = state.ToString() ?? noMessage;
        try {
            if (message == noMessage) {
                WriteErrorToExceptionFolder($"Could not deserialize {message}");
                return;
            }
            logMessageWithCallStack = JsonSerializer.Deserialize<LogMessageWithCallStack>(message);
            if (logMessageWithCallStack == null) {
                WriteErrorToExceptionFolder($"Could not deserialize {message}");
                return;
            }
        } catch {
            WriteErrorToExceptionFolder($"Could not deserialize {message}");
            return;
        }

        var reducesStackOfScopes = ReduceStackAccordingToCallStack(_StackOfScopes, logMessageWithCallStack.MethodNamesInCallStack);
        if (reducesStackOfScopes.Count == 0) {
            WriteErrorToExceptionFolder(string.Format(Properties.Resources.ScopeExistsButOutsideCallStack, string.Join(";", _StackOfScopes)));
            reducesStackOfScopes = _StackOfScopes;
        }

        lock (LockObject) {
            _LogEntries.Add(SimpleLogEntry.Create(logLevel, reducesStackOfScopes.Count == 0 ? _StackOfScopes : reducesStackOfScopes, logMessageWithCallStack.Message));
        }
        simpleLogFlusher.Flush(this, LogSubFolder);
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
        var callStackMethodNames = methodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
        string errorMessage = null;
        lock (LockObject) {
            if (_StackOfScopes.Contains(scope)) {
                errorMessage = string.Format(Properties.Resources.ScopeAlreadyBegan, scope);
            }
            _StackOfScopes.Add(scope);
            if (callStackMethodNames.Count < 2) {
                errorMessage ??= string.Format(Properties.Resources.CallStackTooSmallToFindCreatorMethodName, scope);
                _ScopeToCreatorMethodMapping[scope] = CouldNotBeDetermined;
            } else {
                _ScopeToCreatorMethodMapping[scope] = callStackMethodNames[1];
            }
        }

        if (!string.IsNullOrEmpty(errorMessage)) {
            WriteErrorToExceptionFolder(errorMessage);
        }
        return new LoggingScope(() => OnLoggingScopeDisposing(scope));
    }

    private void WriteErrorToExceptionFolder(string errorMessage) {
        var fileName = _ExceptionFolder.FullName + "\\" + nameof(SimpleLogger) + "-Error-" + Guid.NewGuid().ToString().Replace("-", "") + ".log";
        var contents = errorMessage + "\r\n\r\n" + Environment.StackTrace;
        _ExceptionFolder.CreateIfNecessary();
        File.WriteAllText(fileName, contents);
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