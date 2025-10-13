using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities;

[TestClass]
public class SimpleLogEntryTest {
    [TestMethod]
    public void Create_WithSemicolonInStackElement_ThrowsException() {
        Assert.ThrowsExactly<NotSupportedException>(() => {
            SimpleLogEntry.Create(Microsoft.Extensions.Logging.LogLevel.Information, ["A", "B", "C;D"], "Not a message");
        }, "Stack entry must not contain ';': 'C;D'", "");
    }
}