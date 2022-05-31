using System;
using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities {
    [TestClass]
    public class SimpleLogEntryTest {
        [TestMethod, ExpectedException(typeof(NotSupportedException), "Stack entry must not contain ';': 'C;D'")]
        public void Create_WithSemicolonInStackElement_ThrowsException() {
            SimpleLogEntry.Create(Microsoft.Extensions.Logging.LogLevel.Information, new List<string> { "A", "B", "C;D" }, "Not a message");
        }
    }
}
