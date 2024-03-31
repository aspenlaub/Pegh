using System;
using System.Collections.Generic;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class SimpleLogEntry : ISimpleLogEntry {
    public DateTime LogTime { get; init; }
    public LogLevel LogLevel { get; init; }
    public List<string> Stack { get; init; }
    public string Message { get; init; }
    public bool Flushed { get; set; }

    // ReSharper disable once ParameterTypeCanBeEnumerable.Global
    public static ISimpleLogEntry Create(LogLevel logLevel, IList<string> stack, string message) {
        if (stack.Any(s => s.Contains(";"))) {
            throw new NotSupportedException($"Stack entry must not contain ';': '{stack.First(s => s.Contains(";"))}'");
        }
        var entry = new SimpleLogEntry {
            LogTime = DateTime.Now,
            LogLevel = logLevel,
            Stack = new List<string>(),
            Message = message,
            Flushed = false
        };
        entry.Stack.AddRange(stack);
        return entry;
    }

    public override string ToString() {
        return Message;
    }
}