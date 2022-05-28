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
    private static DateTime CleanupTime = DateTime.Now;
    public HashSet<string> FileNames { get; } = new();

    public void Flush(ISimpleLogger logger, string subFolder) {
        var folder = new Folder(Path.GetTempPath()).SubFolder(subFolder);
        folder.CreateIfNecessary();

        lock (LockObject) {
            var logEntries = logger.FindLogEntries(e => !e.Flushed);
            var ids = logEntries.Select(e => e.Stack[0]).Distinct().ToList();
            foreach (var id in ids) {
                var fileName = folder.FullName + '\\' + id + ".log";
                var entries = logEntries.Where(e => !e.Flushed && e.Stack[0] == id).ToList();
                try {
                    File.AppendAllLines(fileName, entries.Select(Format));
                } catch {
                    return;
                }
                logger.OnEntriesFlushed(entries);
                FileNames.Add(fileName);
            }
        }

        if (DateTime.Now < CleanupTime) { return; }

        var minWriteTime = DateTime.Now.AddDays(-1);
        var files = Directory.GetFiles(folder.FullName, "*.log", SearchOption.TopDirectoryOnly).Where(f => File.GetLastWriteTime(f) < minWriteTime).ToList();
        foreach (var file in files) {
            File.Delete(file);
        }

        CleanupTime = DateTime.Now.AddHours(2);
    }

    private static string Format(ISimpleLogEntry entry) {
        return entry.LogTime.ToString("yyyy-MM-dd") + '\t'
                                                    + entry.LogTime.ToString("HH:mm:ss.ffff") + '\t'
                                                    + string.Join("-", entry.Stack) + '\t'
                                                    + Enum.GetName(typeof(LogLevel), entry.LogLevel) + '\t'
                                                    + entry.Message;
    }

    internal static void ResetCleanupTime() {
        CleanupTime = DateTime.Now;
    }
}