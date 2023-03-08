using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class SimpleLogFlusher : ISimpleLogFlusher {
    private static readonly object LockObject = new();
    private static Dictionary<string, DateTime> FolderToCleanupTime = new();
    public HashSet<string> FileNames { get; } = new();
    public bool FlushIsRequired { get; set; }
    private readonly IExceptionFolderProvider _ExceptionFolderProvider;

    public SimpleLogFlusher(IExceptionFolderProvider exceptionFolderProvider) {
        _ExceptionFolderProvider = exceptionFolderProvider;
    }

    public void Flush(ISimpleLogger logger, string subFolder) {
        var folder = new Folder(Path.GetTempPath()).SubFolder(subFolder);
        folder.CreateIfNecessary();

        lock (LockObject) {
            var logEntries = logger.FindLogEntries(e => !e.Flushed);
            var ids = logEntries.Select(GetTopOfStack).Distinct().ToList();
            foreach (var id in ids) {
                var fileName = folder.FullName + '\\' + StackIdAsFileName(id) + ".log";
                var entries = logEntries.Where(e => !e.Flushed && e.Stack[0] == id).ToList();
                try {
                    File.AppendAllLines(fileName, entries.Select(Format));
                } catch (Exception e) {
                    WriteErrorToExceptionFolder($"Could not flush log entries {e.Message}");
                    return;
                }
                logger.OnEntriesFlushed(entries);
                FileNames.Add(fileName);
            }
        }

        lock (LockObject) {
            if (FolderToCleanupTime.ContainsKey(folder.FullName) && DateTime.Now < FolderToCleanupTime[folder.FullName]) { return; }

            var minWriteTime = DateTime.Now.AddDays(-1);
            var files = Directory.GetFiles(folder.FullName, "*.log", SearchOption.TopDirectoryOnly).Where(f => File.GetLastWriteTime(f) < minWriteTime).ToList();
            foreach (var file in files) {
                try {
                    File.Delete(file);
                    // ReSharper disable once EmptyGeneralCatchClause
                } catch {
                }
            }
        }

        FolderToCleanupTime[folder.FullName] = DateTime.Now.AddHours(2);
        FlushIsRequired = false;
    }

    private static string Format(ISimpleLogEntry entry) {
        return entry.LogTime.ToString("yyyy-MM-dd") + '\t'
            + entry.LogTime.ToString("HH:mm:ss.ffff") + '\t'
            + string.Join(";", entry.Stack) + '\t'
            + Enum.GetName(typeof(LogLevel), entry.LogLevel) + '\t'
            + entry.Message;
    }

    private static string GetTopOfStack(ISimpleLogEntry logEntry) {
        return logEntry.Stack[0];
    }

    private static string StackIdAsFileName(string id) {
        var pos = id.IndexOf('(');
        if (pos < 0) { return id; }
        if (pos + 4 > id.Length) { return id; }

        return id.Substring(0, pos) + '(' + Environment.ProcessId + ')';
    }

    internal static void ResetCleanupTime() {
        lock (LockObject) {
            FolderToCleanupTime = new();
        }
    }

    private void WriteErrorToExceptionFolder(string errorMessage) {
        var fileName = _ExceptionFolderProvider.ExceptionFolder().FullName + "\\" + nameof(SimpleLogFlusher) + "-Error-" + Guid.NewGuid().ToString().Replace("-", "") + ".log";
        var contents = errorMessage + "\r\n\r\n" + Environment.StackTrace;
        File.WriteAllText(fileName, contents);
    }
}