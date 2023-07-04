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
    private IExceptionFolderProvider _ExceptionFolderProvider = new FakeExceptionFolderProvider();

    private const string Id1 = "1";
    private const string Id2 = "2";

    [TestInitialize]
    public void Initialize() {
        var container = new ContainerBuilder().UsePegh("Pegh", new DummyCsArgumentPrompter()).Build();
        _MethodNamesFromStackFramesExtractor = container.Resolve<IMethodNamesFromStackFramesExtractor>();
        var logConfiguration = new LogConfiguration(nameof(SimpleLogReaderTest));
        _Flusher = new SimpleLogFlusher(_ExceptionFolderProvider);
        SimpleLogFlusher.ResetCleanupTime();
        _ExceptionFolderProvider = new FakeExceptionFolderProvider();
        _Logger = new SimpleLogger(logConfiguration, _Flusher, _MethodNamesFromStackFramesExtractor, _ExceptionFolderProvider);
        _LogFolder = new Folder(Path.GetTempPath()).SubFolder(_Logger.LogSubFolder);
        _LogFolder.CreateIfNecessary();
        foreach (var fileName in Directory.GetFiles(_LogFolder.FullName, "*.log")) {
            File.Delete(fileName);
        }
        _StartOfTestTime = DateTime.Now.AddMilliseconds(-20); // Tolerance due to file timestamp precision
        _Sut = new SimpleLogReader();
        VerifyLogWasFlushed();
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
        const string methodName = nameof(ReadLogFile_WithSingleLogEntryOfStackDepth1InFile_ReturnsListWithSingleLogEntry);
        CreateLogEntries(methodName, Id1, 1);
        var fileName = FindLogFile();
        var readLogEntries = _Sut.ReadLogFile(fileName);
        Assert.AreEqual(1, readLogEntries.Count);
        VerifyLogEntry(readLogEntries[0], LogLevel.Error, "Log message #1", new List<string> { methodName + "(" + Id1 + ")" });
    }

    [TestMethod]
    public void ReadLogFile_WithSingleLogEntryOfStackDepth2InFile_ReturnsListWithSingleLogEntry() {
        const string methodName = nameof(ReadLogFile_WithSingleLogEntryOfStackDepth2InFile_ReturnsListWithSingleLogEntry);
        const string callingMethodName = methodName + "_Caller";
        using (_Logger.BeginScope(new SimpleLoggingScopeId { ClassOrMethod = callingMethodName, Id = Id2 })) {
            CreateLogEntries(methodName, Id1, 1);
            var fileName = FindLogFile();
            var readLogEntries = _Sut.ReadLogFile(fileName);
            Assert.AreEqual(1, readLogEntries.Count);
            VerifyLogEntry(readLogEntries[0], LogLevel.Error, "Log message #1", new List<string> { callingMethodName + "(" + Id2 + ")", methodName + "(" + Id1 + ")" });
        }
    }

    [TestMethod]
    public void ReadLogFile_WithTwoLogEntriesOfStackDepth1InFile_ReturnsListWithSingleLogEntry() {
        const string methodName = nameof(ReadLogFile_WithTwoLogEntriesOfStackDepth1InFile_ReturnsListWithSingleLogEntry);
        CreateLogEntries(methodName, Id1, 2);
        var fileName = FindLogFile();
        var readLogEntries = _Sut.ReadLogFile(fileName);
        Assert.AreEqual(2, readLogEntries.Count);
        VerifyLogEntry(readLogEntries[0], LogLevel.Information, "Log message #1", new List<string> { methodName + "(" + Id1 + ")" });
        VerifyLogEntry(readLogEntries[1], LogLevel.Error, "Log message #2", new List<string> { methodName + "(" + Id1 + ")" });
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
        VerifyNoExceptionWasLogged();
        VerifyLogWasFlushed();
        if (fileNames.Count != 0) {
            return fileNames[0];
        }

        Wait.Until(() =>
        {
            fileNames = Directory.GetFiles(_LogFolder.FullName, "*.log").Where(f => File.GetLastWriteTime(f) >= _StartOfTestTime).ToList();
            return fileNames.Any();
        }, TimeSpan.FromSeconds(10));
        Assert.IsTrue(fileNames.Count == 0, "Files found but only after waiting for a longer time");

        fileNames = Directory.GetFiles(_LogFolder.FullName, "*.*").ToList();
        var modifiedAfter = fileNames.Select(f => File.GetLastWriteTime(f).Subtract(_StartOfTestTime)).ToList();
        Assert.IsTrue(modifiedAfter.All(m => m.TotalMilliseconds > 0),
            $"File/-s seem/-s to have been modified before or at start-of-test time ({- modifiedAfter.Min(m => m.TotalMilliseconds)} ms)");
        Assert.IsTrue(fileNames.Count == 0, "No files found, not even other files");
        Assert.IsFalse(fileNames.Count == 0, $"No log files found in log folder but {string.Join("\r\n", fileNames)}");
        return "";
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
                Assert.IsTrue(_Flusher.FileNames.Any(), "Flusher has not registered any file names");
                VerifyLogWasFlushed();
            }
        }
    }

    private void VerifyNoExceptionWasLogged() {
        var exceptionFileNames = Directory.GetFiles(_ExceptionFolderProvider.ExceptionFolder().FullName, "*.*")
            .Where(f => File.GetLastWriteTime(f) >= _StartOfTestTime).ToList();
        var exceptionFileName = exceptionFileNames.FirstOrDefault() ?? "\\";
        Assert.IsTrue(exceptionFileName.Length <= 1, $"An exception was logged {exceptionFileName.Substring(exceptionFileName.LastIndexOf('\\'))}");
    }

    private void VerifyLogWasFlushed() {
        Assert.IsFalse(_Flusher.FlushIsRequired);
    }
}