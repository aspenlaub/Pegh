using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class AssemblyRepositoryTest {
        [TestMethod]
        public void RepositoryIsInAFolderWithWriteAccess() {
            var componentProvider = new ComponentProvider();
            var sut = new AssemblyRepository(componentProvider);
            var folder = sut.Folder;
            Assert.IsTrue(Directory.Exists(folder));
            var fileName = folder + @"\writeaccesstest.txt";
            if (File.Exists(fileName)) {
                File.Delete(fileName);
            }
            Assert.IsFalse(File.Exists(fileName));
            File.WriteAllText(fileName, fileName);
            Assert.IsTrue(File.Exists(fileName));
            File.Delete(fileName);
        }

        [TestMethod]
        public void CanAddAssemblyToRepository() {
            var componentProvider = new ComponentProvider();
            var sut = new AssemblyRepository(componentProvider);
            var folder = sut.Folder;
            var assemblyFileName = GetType().Assembly.Location;
            Assert.IsNotNull(assemblyFileName);
            var shortName = assemblyFileName.Substring(assemblyFileName.LastIndexOf('\\') + 1);
            var assemblyRepositoryFileName = folder + '\\' + shortName;
            if (File.Exists(assemblyRepositoryFileName)) {
                File.Delete(assemblyRepositoryFileName);
            }
            Assert.IsFalse(File.Exists(assemblyRepositoryFileName));
            sut.AddToRepositoryIfNecessary(GetType());
            Assert.IsTrue(File.Exists(assemblyRepositoryFileName));
            sut.AddToRepositoryIfNecessary(GetType());
            Assert.IsTrue(File.Exists(assemblyRepositoryFileName));
            File.Delete(assemblyRepositoryFileName);
            sut.AddToRepositoryIfNecessary(@"C:\This\Folder\Does\Not\Exist\Debug\Something.dll");
        }

        [TestMethod]
        public void CanUpdateIncludeFolder() {
            var componentProvider = new ComponentProvider();
            var sut = new AssemblyRepository(componentProvider);
            var folder = sut.Folder;
            var folderFile = Directory.GetFiles(folder).FirstOrDefault();
            Assert.IsNotNull(folderFile);
            folderFile = folderFile.Substring(folderFile.LastIndexOf('\\') + 1);
            var tempFolder = Path.GetTempPath() + @"Folder\Helper\Test";
            DirectoryDeleter.DeleteDirectory(tempFolder);
            Directory.CreateDirectory(tempFolder);
            var includeFolder = tempFolder + @"\Include";
            Directory.CreateDirectory(includeFolder);
            File.WriteAllText(includeFolder + '\\' + folderFile, "SomeContents");
            File.SetLastWriteTime(includeFolder + '\\' + folderFile, new DateTime(2017, 6, 1));
            Assert.AreNotEqual(File.GetLastWriteTime(folder + '\\' + folderFile), File.GetLastWriteTime(includeFolder + '\\' + folderFile));

            bool success;
            int numberOfRepositoryFiles, numberOfCopiedFiles;

            // Wrong folder
            sut.UpdateIncludeFolder(tempFolder + @"Source.cs", out success, out numberOfRepositoryFiles, out numberOfCopiedFiles);
            Assert.AreNotEqual(File.GetLastWriteTime(folder + '\\' + folderFile), File.GetLastWriteTime(includeFolder + '\\' + folderFile));
            Assert.IsFalse(success);
            Assert.AreEqual(0, numberOfRepositoryFiles);
            Assert.AreEqual(0, numberOfCopiedFiles);

            // Something to do
            sut.UpdateIncludeFolder(tempFolder + @"\Source.cs", out success, out numberOfRepositoryFiles, out numberOfCopiedFiles);
            Assert.AreEqual(File.GetLastWriteTime(folder + '\\' + folderFile), File.GetLastWriteTime(includeFolder + '\\' + folderFile));
            Assert.IsTrue(success);
            Assert.AreEqual(1, numberOfCopiedFiles);

            // Nothing to do
            sut.UpdateIncludeFolder(tempFolder + @"\Source.cs", out success, out numberOfRepositoryFiles, out numberOfCopiedFiles);
            Assert.AreEqual(File.GetLastWriteTime(folder + '\\' + folderFile), File.GetLastWriteTime(includeFolder + '\\' + folderFile));
            Assert.IsTrue(success);
            Assert.AreEqual(0, numberOfCopiedFiles);

            // File not in repository
            var oldNumberOfRepositoryFiles = numberOfRepositoryFiles;
            File.WriteAllText(includeFolder + @"\Library.dll", "SomeContents");
            sut.UpdateIncludeFolder(tempFolder + @"\Source.cs", out success, out numberOfRepositoryFiles, out numberOfCopiedFiles);
            Assert.AreEqual(File.GetLastWriteTime(folder + '\\' + folderFile), File.GetLastWriteTime(includeFolder + '\\' + folderFile));
            Assert.IsTrue(success);
            Assert.AreEqual(oldNumberOfRepositoryFiles + 1, numberOfRepositoryFiles);
            Assert.AreEqual(0, numberOfCopiedFiles);
        }
    }
}