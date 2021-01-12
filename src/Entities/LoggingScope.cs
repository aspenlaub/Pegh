﻿using System;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class LoggingScope : IDisposable {
        private readonly Action vDoDispose;

        public LoggingScope(Action doDispose) {
            vDoDispose = doDispose;
        }

        public void Dispose() {
            vDoDispose();
        }
    }
}
