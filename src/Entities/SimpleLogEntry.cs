using System;
using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class SimpleLogEntry : ISimpleLogEntry {
        public DateTime LogTime { get; init; }
        public LogLevel LogLevel { get; init; }
        public List<string> Stack { get; init; }
        public string Message { get; init; }
        public bool Flushed { get; set; }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static ISimpleLogEntry Create(LogLevel logLevel, IList<string> stack, string message) {
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
    }
}
