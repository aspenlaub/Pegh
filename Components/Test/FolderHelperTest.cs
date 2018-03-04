using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class FolderHelperTest {
        [TestMethod]
        public void FolderIsCreatedIfNecessary() {
            var sut = new FolderHelper();
            var folder = Path.GetTempPath() + @"\Folder\Helper\Test";
            Assert.IsTrue(folder.Contains("Temp"));
            Directory.Delete(folder, true);
            Assert.IsFalse(Directory.Exists(folder));
            sut.CreateIfNecessary(folder);
            Assert.IsTrue(Directory.Exists(folder));
            sut.CreateIfNecessary(folder);
            Assert.IsTrue(Directory.Exists(folder));
        }
    }
}
