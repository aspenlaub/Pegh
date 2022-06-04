using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class SimpleLogReaderTest {
    private ISimpleLogReader _Sut;
    private IMethodNamesFromStackFramesExtractor _MethodNamesFromStackFramesExtractor;
    private ISimpleLogger _Logger;
    private ISimpleLogFlusher _Flusher;
    private IFolder _LogFolder;
    private DateTime _StartOfTestTime;

    private const string MethodName = "WorkAsync";
    private const string CallingMethodName = "Call_WorkAsync";
    private const string Id1 = "1";
    private const string Id2 = "2";

    [TestInitialize]
    public void Initialize() {
        var container = new ContainerBuilder().UsePegh("Pegh", new DummyCsArgumentPrompter()).Build();
        _MethodNamesFromStackFramesExtractor = container.Resolve<IMethodNamesFromStackFramesExtractor>();
        var logConfiguration = new LogConfiguration(nameof(SimpleLogReaderTest));
        _Flusher = new SimpleLogFlusher();
        _Logger = new SimpleLogger(logConfiguration, _Flusher, _MethodNamesFromStackFramesExtractor);
        _LogFolder = new Folder(Path.GetTempPath()).SubFolder(_Logger.LogSubFolder);
        _LogFolder.CreateIfNecessary();
        foreach (var fileName in Directory.GetFiles(_LogFolder.FullName, "*.log")) {
            File.Delete(fileName);
        }
        _StartOfTestTime = DateTime.Now;
        _Sut = new SimpleLogReader();
    }

    [TestMethod]
    public void ReadLogFile_WithEmptyFile_ReturnsEmpty() {
        var fileName = _LogFolder.FullName + @"\empty.log";
        File.WriteAllText(fileName, "");
        var logEntries = _Sut.ReadLogFile(fileName);
        Assert.AreEqual(0, logEntries.Count);
    }

    [TestMethod]
    public void ReadLogFile_WithSingleLogEntryOfStackDepth1InFile_ReturnsListWithSingleLogEntry() {
        CreateLogEntries(MethodName, Id1, 1);
        var fileName = FindLogFile();
        var readLogEntries = _Sut.ReadLogFile(fileName);
        Assert.AreEqual(1, readLogEntries.Count);
        VerifyLogEntry(readLogEntries[0], LogLevel.Error, "Log message #1", new List<string> { MethodName + "(" + Id1 + ")" });
    }

    [TestMethod]
    public void ReadLogFile_WithSingleLogEntryOfStackDepth2InFile_ReturnsListWithSingleLogEntry() {
        using (_Logger.BeginScope(new SimpleLoggingScopeId { ClassOrMethod = CallingMethodName, Id = Id2 })) {
            CreateLogEntries(MethodName, Id1, 1);
            var fileName = FindLogFile();
            var readLogEntries = _Sut.ReadLogFile(fileName);
            Assert.AreEqual(1, readLogEntries.Count);
            VerifyLogEntry(readLogEntries[0], LogLevel.Error, "Log message #1", new List<string> { CallingMethodName + "(" + Id2 + ")", MethodName + "(" + Id1 + ")" });
        }
    }

    [TestMethod]
    public void ReadLogFile_WithTwoLogEntriesOfStackDepth1InFile_ReturnsListWithSingleLogEntry() {
        CreateLogEntries(MethodName, Id1, 2);
        var fileName = FindLogFile();
        var readLogEntries = _Sut.ReadLogFile(fileName);
        Assert.AreEqual(2, readLogEntries.Count);
        VerifyLogEntry(readLogEntries[0], LogLevel.Information, "Log message #1", new List<string> { MethodName + "(" + Id1 + ")" });
        VerifyLogEntry(readLogEntries[1], LogLevel.Error, "Log message #2", new List<string> { MethodName + "(" + Id1 + ")" });
    }

    private void VerifyLogEntry(ISimpleLogEntry logEntry, LogLevel logLevel, string message, IList<string> stack) {
        Assert.AreEqual(logLevel, logEntry.LogLevel);
        Assert.IsTrue(logEntry.LogTime >= _StartOfTestTime);
        Assert.AreEqual(message, logEntry.Message);
        Assert.AreEqual(stack.Count, logEntry.Stack.Count);
        for (var i = 0; i < stack.Count; i++) {
            Assert.AreEqual(stack[i], logEntry.Stack[i]);
        }
    }

    private string FindLogFile() {
        var fileNames = new List<string>();
        Wait.Until(() =>
        {
            fileNames = Directory.GetFiles(_LogFolder.FullName, "*.log").Where(f => File.GetLastWriteTime(f) >= _StartOfTestTime).ToList();
            return fileNames.Any();
        }, TimeSpan.FromMilliseconds(500));
        Assert.IsTrue(fileNames.Count > 0, "No files found");
        return fileNames[0];
    }

    private void CreateLogEntries(string methodName, string logId, int n) {
        using (_Logger.BeginScope(new SimpleLoggingScopeId { ClassOrMethod = methodName, Id = logId })) {
            var methodNamesInStack = _MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
            for (var counter = 0; n > 0; n--) {
                var message = $"Log message #{++counter}";
                switch (n % 3) {
                    case 0:
                        _Logger.LogWarningWithCallStack(message, methodNamesInStack);
                    break;
                    case 1:
                        _Logger.LogErrorWithCallStack(message, methodNamesInStack);
                    break;
                    default:
                        _Logger.LogInformationWithCallStack(message, methodNamesInStack);
                    break;
                }
            }
        }
    }
}