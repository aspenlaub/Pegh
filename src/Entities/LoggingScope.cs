using System;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class LoggingScope : IDisposable {
        private readonly Action DoDispose;

        public LoggingScope(Action doDispose) {
            DoDispose = doDispose;
        }

        public void Dispose() {
            DoDispose();
        }
    }
}
