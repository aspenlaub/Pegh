using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
// ReSharper disable UnusedMemberInSuper.Global

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ISimpleLogger : ILogger {
        string LogSubFolder { get; set; }

        IList<ISimpleLogEntry> FindLogEntries(Func<ISimpleLogEntry, bool> condition);
        void OnEntriesFlushed(IList<ISimpleLogEntry> entries);
    }
}
