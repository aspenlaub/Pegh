using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    [TestClass]
    public class SimpleLogReaderTest {
        private ISimpleLogReader Sut;
        private IMethodNamesFromStackFramesExtractor MethodNamesFromStackFramesExtractor;
        private ISimpleLogger Logger;
        private ISimpleLogFlusher Flusher;
        private IFolder LogFolder;
        private DateTime StartOfTestTime;

        private const string MethodName = "WorkAsync";
        private const string CallingMethodName = "Call_WorkAsync";
        private const string Id1 = "1";
        private const string Id2 = "2";

        [TestInitialize]
        public void Initialize() {
            var container = new ContainerBuilder().UsePegh("Pegh", new DummyCsArgumentPrompter()).Build();
            MethodNamesFromStackFramesExtractor = container.Resolve<IMethodNamesFromStackFramesExtractor>();
            var logConfiguration = new LogConfiguration(nameof(SimpleLogReaderTest));
            Flusher = new SimpleLogFlusher();
            Logger = new SimpleLogger(logConfiguration, Flusher, MethodNamesFromStackFramesExtractor);
            LogFolder = new Folder(Path.GetTempPath()).SubFolder(Logger.LogSubFolder);
            foreach (var fileName in Directory.GetFiles(LogFolder.FullName, "*.log")) {
                File.Delete(fileName);
            }
            Sut = new SimpleLogReader();
            StartOfTestTime = DateTime.Now;
        }

        [TestMethod]
        public void ReadLogFile_WithEmptyFile_ReturnsEmpty() {
            var fileName = LogFolder.FullName + @"\empty.log";
            File.WriteAllText(fileName, "");
            var logEntries = Sut.ReadLogFile(fileName);
            Assert.AreEqual(0, logEntries.Count);
        }

        [TestMethod]
        public void ReadLogFile_WithSingleLogEntryOfStackDepth1InFile_ReturnsListWithSingleLogEntry() {
            CreateLogEntries(MethodName, Id1, 1);
            var fileName = FindLogFile();
            var readLogEntries = Sut.ReadLogFile(fileName);
            Assert.AreEqual(1, readLogEntries.Count);
            VerifyLogEntry(readLogEntries[0], LogLevel.Error, "Log message #1", new List<string> { MethodName + "(" + Id1+ ")" });
        }

        [TestMethod]
        public void ReadLogFile_WithSingleLogEntryOfStackDepth2InFile_ReturnsListWithSingleLogEntry() {
            using (Logger.BeginScope(SimpleLoggingScopeId.Create(CallingMethodName, Id2))) {
                CreateLogEntries(MethodName, Id1, 1);
                var fileName = FindLogFile();
                var readLogEntries = Sut.ReadLogFile(fileName);
                Assert.AreEqual(1, readLogEntries.Count);
                VerifyLogEntry(readLogEntries[0], LogLevel.Error, "Log message #1", new List<string> { CallingMethodName + "(" + Id2 + ")", MethodName + "(" + Id1 + ")" });
            }
        }

        [TestMethod]
        public void ReadLogFile_WithTwoLogEntriesOfStackDepth1InFile_ReturnsListWithSingleLogEntry() {
            CreateLogEntries(MethodName, Id1, 2);
            var fileName = FindLogFile();
            var readLogEntries = Sut.ReadLogFile(fileName);
            Assert.AreEqual(2, readLogEntries.Count);
            VerifyLogEntry(readLogEntries[0], LogLevel.Information, "Log message #1", new List<string> { MethodName + "(" + Id1 + ")" });
            VerifyLogEntry(readLogEntries[1], LogLevel.Error, "Log message #2", new List<string> { MethodName + "(" + Id1 + ")" });
        }

        private void VerifyLogEntry(ISimpleLogEntry logEntry, LogLevel logLevel, string message, IList<string> stack) {
            Assert.AreEqual(logLevel, logEntry.LogLevel);
            Assert.IsTrue(logEntry.LogTime >= StartOfTestTime);
            Assert.AreEqual(message, logEntry.Message);
            Assert.AreEqual(stack.Count, logEntry.Stack.Count);
            for (var i = 0; i < stack.Count; i++) {
                Assert.AreEqual(stack[i], logEntry.Stack[i]);
            }
        }

        private string FindLogFile() {
            var fileNames = Directory.GetFiles(LogFolder.FullName, "*.log").Where(f => File.GetLastWriteTime(f) >= StartOfTestTime).ToList();
            Assert.IsTrue(fileNames.Count > 0, "No files found");
            return fileNames[0];
        }

        private void CreateLogEntries(string methodName, string logId, int n) {
            using (Logger.BeginScope(SimpleLoggingScopeId.Create(methodName, logId))) {
                var methodNamesInStack = MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
                for (var counter = 0; n > 0; n--) {
                    var message = $"Log message #{++ counter}";
                    switch (n % 3) {
                        case 0:
                            Logger.LogWarningWithCallStack(message, methodNamesInStack);
                        break;
                        case 1:
                            Logger.LogErrorWithCallStack(message, methodNamesInStack);
                        break;
                        default:
                            Logger.LogInformationWithCallStack(message, methodNamesInStack);
                        break;
                    }
                }
            }
        }
    }
}
