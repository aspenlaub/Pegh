﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class SimpleLoggerTest {
    private const string NotAMessage = "This is not a message";
    private const int NumberOfLogEntries = 1000;

    [TestMethod, ExpectedException(typeof(Exception), "Attempt to create a log entry without a scope. Use BeginScope<>, and on the same thread")]
    public void SimpleLogger_WithoutScope_ThrowsException() {
        var flusher = new SimpleLogFlusher();
        ISimpleLogger sut = new SimpleLogger(flusher);
        sut.Log(LogLevel.Information, new EventId(0), new Dictionary<string, object>(), null, (_, _) => { return NotAMessage; });
    }

    [TestMethod]
    public void SimpleLogger_WithManyLogCalls_IsWorking() {
        var flusher = new SimpleLogFlusher();
        ISimpleLogger sut = new SimpleLogger(flusher);
        using (sut.BeginScope(SimpleLoggingScopeId.Create("Scope", "A"))) {
            using (sut.BeginScope(SimpleLoggingScopeId.Create("Scope", "B"))) {
                for (var i = 0; i < NumberOfLogEntries; i++) {
                    sut.Log(LogLevel.Information, new EventId(0), new Dictionary<string, object>(), null, (_, _) => { return NotAMessage; });
                }
            }
        }

        var logEntries = sut.FindLogEntries(_ => true);
        Assert.AreEqual(NumberOfLogEntries, logEntries.Count);
        Assert.AreEqual(LogLevel.Information, logEntries[0].LogLevel);
        Assert.AreEqual(2, logEntries[0].Stack.Count);
        Assert.AreEqual("Scope(A)", logEntries[0].Stack[0]);
        Assert.AreEqual("Scope(B)", logEntries[0].Stack[1]);
        Assert.AreEqual(NotAMessage, logEntries[0].Message);

        var fileNames = flusher.FileNames;
        Assert.AreEqual(1, fileNames.Count);
        var fileName = fileNames.First();
        Assert.IsTrue(File.Exists(fileName));
        Assert.IsTrue(fileName.EndsWith(@"\Scope(A).log"));
        Assert.AreEqual(0, logEntries.Count(e => !e.Flushed));

        File.SetLastWriteTime(fileName, DateTime.Now.AddHours(-25));
        SimpleLogFlusher.ResetCleanupTime();
        flusher.Flush(sut, @"AspenlaubLogs\Miscellaneous");
        Assert.IsFalse(File.Exists(fileName));
    }

    [TestMethod]
    public void CanResolveInstance() {
        var container = new ContainerBuilder().UsePegh("Pegh", new DummyCsArgumentPrompter()).Build();
        var logger = container.Resolve<ILogger>();
        Assert.IsNotNull(logger);
        Assert.IsTrue(logger is SimpleLogger);

        var simpleLogger = container.Resolve<ISimpleLogger>();
        Assert.IsNotNull(simpleLogger);
    }
}