using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class SimpleLogReader : ISimpleLogReader {
        public IList<ISimpleLogEntry> ReadLogFile(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                throw new ArgumentNullException(fileName);
            }

            if (!File.Exists(fileName)) {
                throw new FileNotFoundException(fileName);
            }

            var logEntries = new List<ISimpleLogEntry>();
            logEntries.AddRange(File.ReadAllLines(fileName).Select(l => ReadLogEntry(l)));
            return logEntries;
        }

        private ISimpleLogEntry ReadLogEntry(string s) {
            var parts = s.Split("\t").ToList();
            if (parts.Count != 5) {
                throw new Exception($"Could not read log entry: {s}");
            }

            return new SimpleLogEntry {
                LogLevel = Enum.Parse<LogLevel>(parts[3]),
                LogTime = DateTime.Parse(parts[0] + ' ' + parts[1]),
                Message = parts[4],
                Stack = parts[2].Split(";").ToList()
            };
        }
    }
}
