using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities.Test {
    [TestClass]
    public class FolderTest {
        [TestMethod]
        public void TrailingBackslashIsStrippedOff() {
            var tempFolder = Path.GetTempPath();
            Assert.IsTrue(tempFolder.EndsWith("\\"));
            var sut = new Folder(tempFolder);
            Assert.AreEqual(tempFolder.Substring(0, tempFolder.Length - 1), sut.FullName);
            tempFolder = sut.FullName;
            sut = new Folder(tempFolder);
            Assert.AreEqual(tempFolder, sut.FullName);
        }
    }
}
