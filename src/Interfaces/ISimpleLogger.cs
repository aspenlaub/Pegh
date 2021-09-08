using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
// ReSharper disable UnusedMemberInSuper.Global

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ISimpleLogger : ILogger {
        string LogSubFolder { get; }

        IList<ISimpleLogEntry> FindLogEntries(Func<ISimpleLogEntry, bool> condition);
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        void OnEntriesFlushed(IList<ISimpleLogEntry> entries);
    }
}
