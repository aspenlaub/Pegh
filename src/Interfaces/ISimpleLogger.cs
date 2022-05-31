using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface ISimpleLogger : ILogger {
    string LogSubFolder { get; }
    string LogId { get; }

    IList<ISimpleLogEntry> FindLogEntries(Func<ISimpleLogEntry, bool> condition);
    void OnEntriesFlushed(IList<ISimpleLogEntry> entries);
}