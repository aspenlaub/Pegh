using System;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class LoggingScope(Action doDispose) : IDisposable {
    public void Dispose() {
        doDispose();
    }
}