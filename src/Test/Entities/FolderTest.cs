using System.IO;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities {
    [TestClass]
    public class FolderTest {
        [TestMethod]
        public void TrailingBackslashIsStrippedOff() {
            var tempFolder = Path.GetTempPath();
            Assert.IsTrue(tempFolder.EndsWith("\\"));
            var sut = new Folder(tempFolder);
            Assert.AreEqual(tempFolder[..^1], sut.FullName);
            tempFolder = sut.FullName;
            sut = new Folder(tempFolder);
            Assert.AreEqual(tempFolder, sut.FullName);
        }
    }
}
