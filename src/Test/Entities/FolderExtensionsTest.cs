﻿using System.IO;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities;

[TestClass]
public class FolderExtensionsTest {
    [TestMethod]
    public void CanCheckIfExists() {
        var sut = new Folder(Path.GetTempPath());
        Assert.IsTrue(sut.Exists());
        sut = new Folder(@"c:\blabla\tata\googoo");
        Assert.IsFalse(sut.Exists());
    }

    [TestMethod]
    public void CanCheckIfSubFolderExists() {
        var sut = new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp");
        const string subFolder = @"\FolderExtensionsTest";
        if (Directory.Exists(sut.FullName + subFolder)) {
            Directory.Delete(sut.FullName + subFolder);
        }
        Assert.IsFalse(sut.HasSubFolder(subFolder));
        Directory.CreateDirectory(sut.FullName + subFolder);
        Assert.IsTrue(sut.HasSubFolder(subFolder));
        Directory.Delete(sut.FullName + subFolder);
    }

    [TestMethod]
    public void CanGetParentFolder() {
        var sut = new Folder(@"c:\temp\folderextensionstest");
        var parentFolder = sut.ParentFolder();
        Assert.AreEqual(@"c:\temp", parentFolder.FullName);
        parentFolder = parentFolder.ParentFolder();
        Assert.AreEqual(@"c:", parentFolder.FullName);
        parentFolder = parentFolder.ParentFolder();
        Assert.IsNull(parentFolder);
    }

    [TestMethod]
    public void FolderIsCreatedIfNecessary() {
        var folder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder("Folder").SubFolder("Helper").SubFolder("Test");
        Assert.IsTrue(folder.FullName.Contains("Temp"));
        if (folder.Exists()) {
            Directory.Delete(folder.FullName, true);
        }
        Assert.IsFalse(folder.Exists());
        folder.CreateIfNecessary();
        Assert.IsTrue(folder.Exists());
        folder.CreateIfNecessary();
        Assert.IsTrue(folder.Exists());
    }

    [TestMethod]
    public void CanDeleteLinksInFolder() {
        var folder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder("FolderWithLinks");
        folder.CreateIfNecessary();
        var subFolder = folder.SubFolder("ThisIsNotLinked");
        subFolder.CreateIfNecessary();
        var linkFile = folder.FullName + @"\ThisIsLinked.lnk";
        File.WriteAllText(linkFile, "");
        Assert.IsTrue(File.Exists(linkFile));
        folder.DeleteLinks();
        Assert.IsTrue(subFolder.Exists());
        Assert.IsFalse(File.Exists(linkFile));
        Directory.Delete(subFolder.FullName);
        Directory.Delete(folder.FullName);
    }
}