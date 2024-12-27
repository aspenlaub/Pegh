using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class SimpleLoggerTest {
    private const string _notAMessage = "This is not a message";
    private const int _numberOfLogEntries = 1000;

    private readonly IMethodNamesFromStackFramesExtractor _MethodNamesFromStackFramesExtractor = new MethodNamesFromStackFramesExtractor();

    private SimpleLogFlusher _Flusher;
    private ISimpleLogger _Sut;
    private DateTime _StartOfTestTime;
    private readonly IExceptionFolderProvider _ExceptionFolderProvider = new FakeExceptionFolderProvider();

    [TestInitialize]
    public void Initialize() {
        _Flusher = new SimpleLogFlusher(_ExceptionFolderProvider);
        SimpleLogFlusher.ResetCleanupTime();
        _StartOfTestTime = DateTime.Now;
    }

    private ILogConfiguration CreateLogConfiguration(string pseudoApplicationName) {
        return new LogConfiguration(pseudoApplicationName);
    }

    [TestMethod]
    public void Log_CalledManyTimes_IsWorking() {
        _Sut = new SimpleLogger(CreateLogConfiguration(nameof(Log_CalledManyTimes_IsWorking)), _Flusher,
            _MethodNamesFromStackFramesExtractor, _ExceptionFolderProvider);
        using (_Sut.BeginScope(new SimpleLoggingScopeId { ClassOrMethod =  "Scope", Id = "A" })) {
            using (_Sut.BeginScope(new SimpleLoggingScopeId { ClassOrMethod = "Scope", Id = "B" })) {
                for (var i = 0; i < _numberOfLogEntries; i++) {
                    _Sut.LogInformationWithCallStack(_notAMessage, _MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames());
                }
            }
        }

        VerifyNoExceptionWasLogged();
        VerifyLogWasFlushedOrNot(false);

        var logEntries = _Sut.FindLogEntries(_ => true);
        Assert.AreEqual(_numberOfLogEntries, logEntries.Count);
        Assert.AreEqual(LogLevel.Information, logEntries[0].LogLevel);
        Assert.AreEqual(2, logEntries[0].Stack.Count);
        Assert.AreEqual("Scope(A)", logEntries[0].Stack[0]);
        Assert.AreEqual("Scope(B)", logEntries[0].Stack[1]);
        Assert.AreEqual(_notAMessage, logEntries[0].Message);

        var fileNames = _Flusher.FileNames;
        Assert.AreEqual(1, fileNames.Count);
        var fileName = fileNames.First();
        Assert.IsTrue(File.Exists(fileName));
        Assert.IsTrue(fileName.EndsWith(@"\Scope(A).log"));
        Assert.AreEqual(0, logEntries.Count(e => !e.Flushed));

        File.SetLastWriteTime(fileName, DateTime.Now.AddHours(-25));
        SimpleLogFlusher.ResetCleanupTime();
        _Flusher.Flush(_Sut, _Sut.LogSubFolder);
        VerifyLogWasFlushedOrNot(false);
        Assert.IsFalse(File.Exists(fileName));
    }


    [TestMethod]
    public void Log_CalledManyTimesWithRandomId_IsWorking() {
        _Sut = new SimpleLogger(CreateLogConfiguration(nameof(Log_CalledManyTimesWithRandomId_IsWorking)), _Flusher,
            _MethodNamesFromStackFramesExtractor, _ExceptionFolderProvider);
        using (_Sut.BeginScope(SimpleLoggingScopeId.Create("Scope"))) {
            using (_Sut.BeginScope(SimpleLoggingScopeId.Create("Scope"))) {
                for (var i = 0; i < _numberOfLogEntries; i++) {
                    _Sut.LogInformationWithCallStack(_notAMessage, _MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames());
                }
            }
        }
    }

    [TestMethod]
    public async Task Log_WithinParallelDifferentTasks_IsWorking() {
        _Sut = new SimpleLogger(CreateLogConfiguration(nameof(Log_WithinParallelDifferentTasks_IsWorking)), _Flusher,
                                _MethodNamesFromStackFramesExtractor, _ExceptionFolderProvider);
        var tasks = new List<Task> {
            new ImLogging(TimeSpan.FromMilliseconds(77), DateTime.Now.AddSeconds(4), _Sut, _MethodNamesFromStackFramesExtractor).ImLoggingWorkAsync(),
            new ImLoggingToo(TimeSpan.FromMilliseconds(222), DateTime.Now.AddSeconds(7), _Sut, _MethodNamesFromStackFramesExtractor).ImLoggingWorkTooAsync()
        };
        await Task.WhenAll(tasks);
    }

    [TestMethod]
    public async Task Log_WithinParallelSimilarTasks_IsWorking() {
        _Sut = new SimpleLogger(CreateLogConfiguration(nameof(Log_WithinParallelSimilarTasks_IsWorking)), _Flusher,
                                _MethodNamesFromStackFramesExtractor, _ExceptionFolderProvider);
        var tasks = new List<Task> {
            new ImLogging(TimeSpan.FromMilliseconds(77), DateTime.Now.AddSeconds(4), _Sut, _MethodNamesFromStackFramesExtractor).ImLoggingWorkAsync(),
            new ImLogging(TimeSpan.FromMilliseconds(222), DateTime.Now.AddSeconds(7), _Sut, _MethodNamesFromStackFramesExtractor).ImLoggingWorkAsync()
        };
        await Task.WhenAll(tasks);
    }

    [TestMethod]
    public void Constructor_WithLogConfiguration_ProducesLoggerWithConfiguredLogId() {
        var logConfiguration = CreateLogConfiguration(nameof(Constructor_WithLogConfiguration_ProducesLoggerWithConfiguredLogId));
        _Sut = new SimpleLogger(logConfiguration, _Flusher,
            _MethodNamesFromStackFramesExtractor, _ExceptionFolderProvider);
        Assert.AreEqual(logConfiguration.LogId, _Sut.LogId);
    }

    [TestMethod]
    public void Constructor_WithLogConfiguration_ProducesLoggerWithConfiguredSubFolder() {
        ISimpleLogger sut = new SimpleLogger(new LogConfiguration(nameof(Constructor_WithLogConfiguration_ProducesLoggerWithConfiguredSubFolder)),
            new SimpleLogFlusher(_ExceptionFolderProvider), new MethodNamesFromStackFramesExtractor(), _ExceptionFolderProvider);
        Assert.AreEqual(@"AspenlaubLogs\" + nameof(Constructor_WithLogConfiguration_ProducesLoggerWithConfiguredSubFolder), sut.LogSubFolder);
    }

    private void VerifyNoExceptionWasLogged() {
        var exceptionFileNames = Directory.GetFiles(_ExceptionFolderProvider.ExceptionFolder().FullName, "*.*").Where(f => File.GetLastWriteTime(f) >= _StartOfTestTime).ToList();
        var exceptionFileName = exceptionFileNames.FirstOrDefault() ?? "\\";
        Assert.IsTrue(exceptionFileName.Length <= 1, $"An exception was logged {exceptionFileName.Substring(exceptionFileName.LastIndexOf('\\'))}");
    }

    private void VerifyLogWasFlushedOrNot(bool was) {
        Assert.AreEqual(was, _Flusher.FlushIsRequired);
    }
}