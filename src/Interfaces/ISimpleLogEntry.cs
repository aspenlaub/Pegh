using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ISimpleLogEntry {
        DateTime LogTime { get; set; }
        LogLevel LogLevel { get; set; }
        List<string> Stack { get; set; }
        string Message { get; set; }
        bool Flushed { get; set; }
    }
}