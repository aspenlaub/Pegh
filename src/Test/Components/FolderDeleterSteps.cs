using System.IO;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    [Binding]
    public class FolderDeleterSteps {
        protected IFolder Folder, SubFolder;
        protected FolderDeleter Sut;
        protected bool CanDeleteFolder, DoubleCheckOkay;

        public FolderDeleterSteps() {
            Sut = new FolderDeleter {
                IgnoreUserTempFolder = true
            };
        }

        [AfterScenario("FolderDeleter")]
        public void CleanUp() {
            if (!Directory.Exists(Folder.FullName)) { return; }

            Directory.Delete(Folder.FullName, true);
        }

        [Given(@"I have a folder beneath the user's temp folder but not immediately recognizable as such")]
        public void GivenIHaveAFolderBeneathTheUserSTempFolder() {
            Folder = new Folder(Path.GetTempPath().Replace(@"\", @"\.\") + nameof(FolderDeleterSteps));
            if (Directory.Exists(Folder.FullName)) {
                Directory.Delete(Folder.FullName, true);
            }
            Directory.CreateDirectory(Folder.FullName);
            SubFolder = Folder.SubFolder(@"\SubFolder");
            Directory.CreateDirectory(SubFolder.FullName);
        }

        [Given(@"the folder contains (.*) files")]
        public void GivenTheFolderContainsFiles(int p0) {
            do {
                var missing = p0 - Directory.GetFiles(Folder.FullName, "*.*", SearchOption.AllDirectories).Length;
                if (missing <= 0) {
                    break;
                }

                var fileName = (missing % 2 == 1 ? Folder : SubFolder).FullName + @"\File" + missing + ".txt";
                File.WriteAllText(fileName, fileName);
            } while (true);
        }

        [Given(@"there is a \.git subfolder")]
        public void GivenThereIsA_GitSubfolder() {
            Directory.CreateDirectory(Folder.GitSubFolder().FullName);
        }

        [Given(@"the folder ends with \\obj")]
        public void GivenTheFolderEndsWithObj() {
            var newFolder = new Folder(Path.GetTempPath() + @"obj");
            if (Directory.Exists(newFolder.FullName)) {
                Directory.Delete(newFolder.FullName, true);
            }
            Directory.Move(Folder.FullName, newFolder.FullName);
            Folder = newFolder;
            SubFolder = Folder.SubFolder(@"\SubFolder");
        }

        [Given(@"the folder ends with \\bin")]
        public void GivenTheFolderEndsWithBin() {
            var newFolder = new Folder(Path.GetTempPath() + @"bin");
            if (Directory.Exists(newFolder.FullName)) {
                Directory.Delete(newFolder.FullName, true);
            }
            Directory.Move(Folder.FullName, newFolder.FullName);
            Folder = newFolder;
            SubFolder = Folder.SubFolder(@"\SubFolder");
        }

        [Given(@"the folder is located beneath c:\\temp")]
        public void GivenTheFolderIsLocatedBeneathCTemp() {
            var newFolder = new Folder(@"c:\temp\" + nameof(FolderDeleterSteps));
            if (Directory.Exists(newFolder.FullName)) {
                Directory.Delete(newFolder.FullName, true);
            }
            Directory.Move(Folder.FullName, newFolder.FullName);
            Folder = newFolder;
            SubFolder = Folder.SubFolder(@"\SubFolder");
        }

        [When(@"I ask the folder deleter if I am allowed to delete the folder")]
        public void WhenIAskTheFolderDeleteIfIAmAllowedToDeleteTheFolder() {
            CanDeleteFolder = Sut.CanDeleteFolder(Folder);
        }

        [When(@"I ask the folder deleter to double-check this")]
        public void WhenIAskTheFolderDeleterToDouble_CheckThis() {
            DoubleCheckOkay = CanDeleteFolder == Sut.CanDeleteFolderDoubleCheck(Folder);
        }

        [When(@"I ask the folder deleter to delete the folder")]
        public void WhenIAskTheFolderDeleterToDeleteTheFolder() {
            try {
                Sut.DeleteFolder(Folder);
            } catch { // ignored
            }
        }

        [Then(@"the result is yes")]
        public void ThenTheResultIsYes() {
            Assert.IsTrue(CanDeleteFolder);
        }

        [Then(@"the double-check agrees")]
        public void ThenTheDouble_CheckAgrees() {
            Assert.IsTrue(DoubleCheckOkay);
        }

        [Then(@"the folder is gone")]
        public void ThenTheFolderIsGone() {
            Assert.IsFalse(Directory.Exists(Folder.FullName));
        }

        [Then(@"the result is no")]
        public void ThenTheResultIsNo() {
            Assert.IsFalse(CanDeleteFolder);
        }

        [Then(@"the folder is still there")]
        public void ThenTheFolderIsStillThere() {
            Assert.IsTrue(Directory.Exists(Folder.FullName));
        }
    }
}
