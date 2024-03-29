﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class FolderResolverTest {
    private readonly IFolderResolver _Sut;

    private static IContainer Container { get; set; }
    private static IContainer ProductionContainer { get; set; }

    public FolderResolverTest() {
        var machineDrives = new MachineDrives {
            new() { Machine = Environment.MachineName, Name = "SomeDrive", Drive = "e" }
        };
        var logicalFolders = new LogicalFolders {
            new() { Name = "SomeLogicalFolder", Folder = @"$(SomeDrive)\Logical\Folder" },
            new() { Name = "SomeOtherLogicalFolder", Folder = @"$(SomeLogicalFolder)\Other" }
        };

        var secretRepositoryMock = new Mock<ISecretRepository>();
        secretRepositoryMock.Setup(s => s.GetAsync(It.IsAny<MachineDrivesSecret>(), It.IsAny<IErrorsAndInfos>())).Returns(Task.FromResult(machineDrives));
        secretRepositoryMock.Setup(s => s.GetAsync(It.IsAny<LogicalFoldersSecret>(), It.IsAny<IErrorsAndInfos>())).Returns(Task.FromResult(logicalFolders));

        var builder = new ContainerBuilder().UseForPeghTest(secretRepositoryMock.Object);
        Container = builder.Build();

        builder = new ContainerBuilder().UsePegh("Pegh", new DummyCsArgumentPrompter());
        ProductionContainer = builder.Build();

        _Sut = Container.Resolve<IFolderResolver>();
    }

    [TestMethod]
    public async Task DriveResolvesToActualDrive() {
        IErrorsAndInfos errorsAndInfos = new ErrorsAndInfos();
        Assert.AreEqual(@"E:", (await _Sut.ResolveAsync(@"$(SomeDrive)", errorsAndInfos)).FullName);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        Assert.AreEqual(@"E:", (await _Sut.ResolveAsync(@"$(SomeDrive)\", errorsAndInfos)).FullName);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
    }

    [TestMethod]
    public async Task CanResolveLogicalFolder() {
        var errorsAndInfos = new ErrorsAndInfos();
        Assert.AreEqual(@"E:\Logical\Folder", (await _Sut.ResolveAsync(@"$(SomeLogicalFolder)", errorsAndInfos)).FullName);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        Assert.AreEqual(@"E:\Logical\Folder\Other", (await _Sut.ResolveAsync(@"$(SomeOtherLogicalFolder)", errorsAndInfos)).FullName);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
    }

    [TestMethod]
    public async Task CanUseRealResolver() {
        var sut = ProductionContainer.Resolve<IFolderResolver>();
        var errorsAndInfos = new ErrorsAndInfos();
        var folder = await sut.ResolveAsync("$(MainUserFolder)", errorsAndInfos);
        Assert.IsTrue(folder.SubFolder("CSharp").Exists());
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        Assert.IsTrue(folder.SubFolder("GitHub").Exists());
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
    }

    [TestMethod]
    public async Task ProperErrorMessagesAreReturnedIfPlaceholderIsNotDefined() {
        var sut = ProductionContainer.Resolve<IFolderResolver>();
        var errorsAndInfos = new ErrorsAndInfos();
        await sut.ResolveAsync("$(CSharpDrive)/$(OopsNotExisting)/$(OopsNotExistingEither)", errorsAndInfos);
        Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("$(OopsNotExisting)")));
        Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("$(OopsNotExistingEither)")));
    }
}