using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ISimpleLogEntry {
        DateTime LogTime { get; }
        LogLevel LogLevel { get; }
        List<string> Stack { get; }
        string Message { get; }
        bool Flushed { get; set; }
    }
}