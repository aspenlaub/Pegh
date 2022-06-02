using System;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class LoggingScope : IDisposable {
    private readonly Action _DoDispose;

    public LoggingScope(Action doDispose) {
        _DoDispose = doDispose;
    }

    public void Dispose() {
        _DoDispose();
    }
}