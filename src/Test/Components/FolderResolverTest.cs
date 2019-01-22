using System;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    [TestClass]
    public class FolderResolverTest {
        private IFolderResolver vSut;

        [TestInitialize]
        public void Initialize() {
            var machineDrives = new MachineDrives {
                new MachineDrive { Machine = Environment.MachineName, Name = "SomeDrive", Drive = "e" }
            };
            var logicalFolders = new LogicalFolders {
                new LogicalFolder { Name = "SomeLogicalFolder", Folder = @"$(SomeDrive)\Logical\Folder" },
                new LogicalFolder { Name = "SomeOtherLogicalFolder", Folder = @"$(SomeLogicalFolder)\Other" }
            };
            var componentProviderMock = new Mock<IComponentProvider>();
            var secretRepositoryMock = new Mock<ISecretRepository>();
            secretRepositoryMock.Setup(s => s.GetAsync(It.IsAny<MachineDrivesSecret>(), It.IsAny<IErrorsAndInfos>())).Returns(Task.FromResult(machineDrives));
            secretRepositoryMock.Setup(s => s.GetAsync(It.IsAny<LogicalFoldersSecret>(), It.IsAny<IErrorsAndInfos>())).Returns(Task.FromResult(logicalFolders));
            componentProviderMock.SetupGet(c => c.SecretRepository).Returns(secretRepositoryMock.Object);

            vSut = new FolderResolver(componentProviderMock.Object);
        }

        [TestMethod]
        public void DriveResolvesToActualDrive() {
            var errorsAndInfos = new ErrorsAndInfos();
            Assert.AreEqual(@"E:", vSut.Resolve(@"$(SomeDrive)", errorsAndInfos).FullName);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
            Assert.AreEqual(@"E:", vSut.Resolve(@"$(SomeDrive)\", errorsAndInfos).FullName);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        }

        [TestMethod]
        public void CanResolveLogicalFolder() {
            var errorsAndInfos = new ErrorsAndInfos();
            Assert.AreEqual(@"E:\Logical\Folder", vSut.Resolve(@"$(SomeLogicalFolder)", errorsAndInfos).FullName);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
            Assert.AreEqual(@"E:\Logical\Folder\Other", vSut.Resolve(@"$(SomeOtherLogicalFolder)", errorsAndInfos).FullName);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        }

        [TestMethod]
        public void CanUseRealResolver() {
            IComponentProvider componentProvider = new ComponentProvider();
            var sut = componentProvider.FolderResolver;
            var errorsAndInfos = new ErrorsAndInfos();
            var folder = sut.Resolve("$(MainUserFolder)", errorsAndInfos);
            Assert.IsTrue(folder.SubFolder("CSharp").Exists());
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
            Assert.IsTrue(folder.SubFolder("GitHub").Exists());
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        }

        [TestMethod]
        public void ProperErrorMessagesAreReturnedIfPlaceholderIsNotDefined() {
            IComponentProvider componentProvider = new ComponentProvider();
            var sut = componentProvider.FolderResolver;
            var errorsAndInfos = new ErrorsAndInfos();
            sut.Resolve("$(CSharpDrive)/$(OopsNotExisting)/$(OopsNotExistingEither)", errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("$(OopsNotExisting)")));
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("$(OopsNotExistingEither)")));
        }
    }
}
