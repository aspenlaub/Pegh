using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components; 

[TestClass]
public class SimpleLogFlusherTest {
    private IExceptionFolderProvider _ExceptionFolderProvider = new FakeExceptionFolderProvider();

    [TestInitialize]
    public void Initialize() {
        SimpleLogFlusher.ResetCleanupTime();
    }

    [TestMethod]
    public void Flush_WithRandomScope_ProducesFileWithoutRandomComponent() {
        var sut = new SimpleLogFlusher(_ExceptionFolderProvider);
        var loggerMock = new Mock<ISimpleLogger>();
        var logEntries = new List<ISimpleLogEntry> {
            new SimpleLogEntry {
                LogLevel = LogLevel.Information, Message = nameof(SimpleLogFlusherTest), Stack = new List<string> { nameof(SimpleLogFlusherTest) + "(AE34C4AB53B9)BE40" }
            }
        };
        loggerMock.Setup(l => l.FindLogEntries(It.IsAny<Func<ISimpleLogEntry, bool>>())).Returns(logEntries);
        const string subFolder = @"AspenlaubLogs\" + nameof(SimpleLogFlusherTest);
        var folder = new Folder(Path.GetTempPath()).SubFolder(subFolder);
        folder.CreateIfNecessary();
        foreach (var fileName in Directory.GetFiles(folder.FullName, "*.log")) {
            File.Delete(fileName);
        }
        sut.Flush(loggerMock.Object, subFolder);
        var fileNames = Directory.GetFiles(folder.FullName, "*.log").ToList();
        Assert.AreEqual(1, fileNames.Count);
        Assert.AreEqual(folder.FullName + '\\' + nameof(SimpleLogFlusherTest) + '(' + Environment.ProcessId + ").log", fileNames[0]);
        Assert.IsTrue(File.ReadAllText(fileNames[0]).Contains(logEntries[0].Message));
    }
}