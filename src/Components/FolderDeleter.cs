﻿using System;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class FolderDeleter : IFolderDeleter {
        protected readonly string Obj = @"\obj", CTemp = @"C:\Temp\", TestResults = @"\TestResults";
        protected readonly int MinimumFolderNameLength = 20;

        public bool CanDeleteFolder(IFolder folder, out IFolderDeleteGates folderDeleteGates) {
            folderDeleteGates = new FolderDeleteGates {
                FolderNameIsLongEnough = folder.FullName.Length > MinimumFolderNameLength,
                EndsWithObj = folder.FullName.EndsWith(Obj),
                CTemp = folder.FullName.StartsWith(CTemp, StringComparison.OrdinalIgnoreCase),
                NotTooManyFilesInFolder = true,
                IsGitCheckOutFolder = folder.GitSubFolder().Exists(),
                UserTemp = folder.FullName.StartsWith(Path.GetTempPath()),
                TestResults = folder.FullName.Contains(TestResults)
            };
            if (folderDeleteGates.IsGitCheckOutFolder) { return true; }
            if (!folderDeleteGates.FolderNameIsLongEnough) { return false; }
            if (folderDeleteGates.EndsWithObj) { return true; }
            if (folderDeleteGates.CTemp) { return true; }
            if (folderDeleteGates.UserTemp) { return true; }
            if (folderDeleteGates.TestResults) { return true; }

            var directoryInfo = new DirectoryInfo(folder.FullName);
            var files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();
            folderDeleteGates.NotTooManyFilesInFolder = files.Count <= 10;

            return folderDeleteGates.NotTooManyFilesInFolder;
        }

        public bool CanDeleteFolder(IFolder folder) {
            IFolderDeleteGates folderDeleteGates;
            return CanDeleteFolder(folder, out folderDeleteGates);
        }

        public bool CanDeleteFolderDoubleCheck(IFolder folder) {
            if (folder.GitSubFolder().Exists()) { return true; }
            if (folder.FullName.Length <= MinimumFolderNameLength) { return false; }
            if (folder.FullName.EndsWith(Obj)) { return true; }
            if (folder.FullName.StartsWith(CTemp, StringComparison.OrdinalIgnoreCase)) { return true; }
            if (folder.FullName.StartsWith(Path.GetTempPath())) { return true; }
            if (folder.FullName.Contains(TestResults)) { return true; }

            var directoryInfo = new DirectoryInfo(folder.FullName);
            var files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();

            return files.Count <= 10;
        }

        public void DeleteFolder(IFolder folder) {
            if (!CanDeleteFolder(folder)) {
                throw new ArgumentException("This folder cannot be deleted, use CanDeleteDirectoy to find out why");
            }
            if (!CanDeleteFolderDoubleCheck(folder)) {
                throw new ArgumentException("Double check on CanDeleteDirectoy FAILED");
            }

            MakeFilesDeletable(folder);
            Directory.Delete(folder.FullName, true);
        }

        protected void MakeFilesDeletable(IFolder folder) {
            if (!folder.GitSubFolder().Exists()) { return; }

            folder = folder.GitSubFolder();
            var directoryInfo = new DirectoryInfo(folder.FullName) { Attributes = FileAttributes.Normal };
            directoryInfo.Attributes = directoryInfo.Attributes & ~FileAttributes.Hidden;
            foreach (var fileInfo in directoryInfo.GetFileSystemInfos("*", SearchOption.AllDirectories).Where(fileName => (fileName.Attributes & FileAttributes.Normal) == 0)) {
                fileInfo.Attributes = FileAttributes.Normal;
            }
        }
    }
}
