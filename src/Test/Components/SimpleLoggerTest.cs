﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

    private readonly IMethodNamesFromStackFramesExtractor MethodNamesFromStackFramesExtractor = new MethodNamesFromStackFramesExtractor();

    private SimpleLogFlusher Flusher;
    private SimpleLogger Sut;

    [TestInitialize]
    public void Initialize() {
        Flusher = new SimpleLogFlusher();
    }

    private ILogConfiguration CreateLogConfiguration(string pseudoApplicationName) {
        return new LogConfiguration(pseudoApplicationName);
    }

    [TestMethod, ExpectedException(typeof(Exception), "Attempt to create a log entry without a scope. Use BeginScope<>, and on the same thread")]
    public void SimpleLogger_WithoutScope_ThrowsException() {
        Sut = new SimpleLogger(CreateLogConfiguration(nameof(SimpleLogger_WithoutScope_ThrowsException)), Flusher, MethodNamesFromStackFramesExtractor);
        Sut.Log(LogLevel.Information, new EventId(0), new Dictionary<string, object>(), null, (_, _) => { return NotAMessage; });
    }

    [TestMethod]
    public void SimpleLogger_WithManyLogCalls_IsWorking() {
        Sut = new SimpleLogger(CreateLogConfiguration(nameof(SimpleLogger_WithManyLogCalls_IsWorking)), Flusher, MethodNamesFromStackFramesExtractor);
        using (Sut.BeginScope(SimpleLoggingScopeId.Create("Scope", "A"))) {
            using (Sut.BeginScope(SimpleLoggingScopeId.Create("Scope", "B"))) {
                for (var i = 0; i < NumberOfLogEntries; i++) {
                    Sut.Log(LogLevel.Information, new EventId(0), new Dictionary<string, object>(), null, (_, _) => { return NotAMessage; });
                }
            }
        }

        var logEntries = Sut.FindLogEntries(_ => true);
        Assert.AreEqual(NumberOfLogEntries, logEntries.Count);
        Assert.AreEqual(LogLevel.Information, logEntries[0].LogLevel);
        Assert.AreEqual(2, logEntries[0].Stack.Count);
        Assert.AreEqual("Scope(A)", logEntries[0].Stack[0]);
        Assert.AreEqual("Scope(B)", logEntries[0].Stack[1]);
        Assert.AreEqual(NotAMessage, logEntries[0].Message);

        var fileNames = Flusher.FileNames;
        Assert.AreEqual(1, fileNames.Count);
        var fileName = fileNames.First();
        Assert.IsTrue(File.Exists(fileName));
        Assert.IsTrue(fileName.EndsWith(@"\Scope(A).log"));
        Assert.AreEqual(0, logEntries.Count(e => !e.Flushed));

        File.SetLastWriteTime(fileName, DateTime.Now.AddHours(-25));
        SimpleLogFlusher.ResetCleanupTime();
        Flusher.Flush(Sut, Sut.LogSubFolder);
        Assert.IsFalse(File.Exists(fileName));
    }

    [TestMethod]
    public void PeghContainer_ReturnsSimpleLogger() {
        var sut = new ContainerBuilder().UsePegh("Pegh", new DummyCsArgumentPrompter()).Build();
        var logger = sut.Resolve<ILogger>();
        Assert.IsNotNull(logger);
        Assert.IsTrue(logger is SimpleLogger);

        var simpleLogger = sut.Resolve<ISimpleLogger>();
        Assert.IsNotNull(simpleLogger);
    }

    [TestMethod]
    public async Task SimpleLogger_WithParallelTasks_IsWorking() {
        Sut = new SimpleLogger(CreateLogConfiguration(nameof(SimpleLogger_WithParallelTasks_IsWorking)), Flusher, MethodNamesFromStackFramesExtractor);
        var tasks = new List<Task> {
            new ImLogging(TimeSpan.FromMilliseconds(77), DateTime.Now.AddSeconds(4), Sut).ImLoggingWorkAsync(),
            new ImLoggingToo(TimeSpan.FromMilliseconds(222), DateTime.Now.AddSeconds(7), Sut).ImLoggingWorkTooAsync()
        };
        await Task.WhenAll(tasks);
    }
}