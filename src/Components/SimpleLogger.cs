﻿using System;
using System.Collections.Generic;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class SimpleLogger : ISimpleLogger {
        private const int MaxLogEntries = 10000;

        private static readonly object LockObject = new();

        private readonly List<ISimpleLogEntry> vLogEntries;
        private readonly IList<string> vStack;
        private readonly ISimpleLogFlusher vSimpleLogFlusher;

        public string LogSubFolder { get; set; } = @"AspenlaubLogs\Miscellaneous";

        public bool Enabled { get; set; }

        public SimpleLogger(ISimpleLogFlusher simpleLogFlusher) {
            vLogEntries = new List<ISimpleLogEntry>();
            vStack = new List<string>();
            vSimpleLogFlusher = simpleLogFlusher;
            Enabled = true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            if (!Enabled) { return; }

            lock (LockObject) {
                vLogEntries.Add(SimpleLogEntry.Create(logLevel, vStack, formatter(state, exception)));
            }
            vSimpleLogFlusher.Flush(this, LogSubFolder);
        }

        public bool IsEnabled(LogLevel logLevel) {
            return Enabled;
        }

        public IDisposable BeginScope<TState>(TState state) {
            if (!(state is ISimpleLoggingScopeId loggingScope)) {
                return new LoggingScope(() => { });
            }

            var stackEntry = $"{loggingScope.Class}({loggingScope.Id})";
            lock (LockObject) {
                vStack.Add(stackEntry);
            }
            return new LoggingScope(() => {
                lock (LockObject) {
                    vStack.Remove(stackEntry);
                }
            });
        }

        public IList<ISimpleLogEntry> FindLogEntries(Func<ISimpleLogEntry, bool> condition) {
            lock (LockObject) {
                var logEntries = new List<ISimpleLogEntry>();
                logEntries.AddRange(vLogEntries.Where(condition));
                return logEntries;
            }
        }

        public void OnEntriesFlushed(IList<ISimpleLogEntry> entries) {
            lock (LockObject) {
                foreach (var entry in entries) {
                    entry.Flushed = true;
                }

                if (vLogEntries.Count < MaxLogEntries) { return; }

                int i;
                for (i = 0; vLogEntries[i].Flushed && i < MaxLogEntries / 2; i++) {
                }
                vLogEntries.RemoveRange(0, i);
            }
        }
    }
}
