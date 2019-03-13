using System;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class FolderUpdater : IFolderUpdater {
        private const int MinimumBinaryFileSizeInBytes = 4000;

        public void UpdateFolder(IFolder sourceFolder, IFolder destinationFolder, FolderUpdateMethod folderUpdateMethod, IErrorsAndInfos errorsAndInfos) {
            if (folderUpdateMethod != FolderUpdateMethod.AssembliesButNotIfOnlySlightlyChanged && folderUpdateMethod != FolderUpdateMethod.AssembliesEvenIfOnlySlightlyChanged) {
                throw new NotImplementedException("Update method is not implemented");
            }

            if (!destinationFolder.Exists()) {
                Directory.CreateDirectory(destinationFolder.FullName);
            }
            foreach (var sourceFileInfo in Directory.GetFiles(sourceFolder.FullName, "*.*", SearchOption.AllDirectories).Select(f => new FileInfo(f))) {
                var destinationFileInfo = new FileInfo(destinationFolder.FullName + '\\' + sourceFileInfo.FullName.Substring(sourceFolder.FullName.Length));
                if (File.Exists(destinationFileInfo.FullName)) {
                    if (sourceFileInfo.Length == 0 && destinationFileInfo.Length == 0) { continue; }

                    if (sourceFileInfo.Length == destinationFileInfo.Length) {
                        var sourceContents = File.ReadAllBytes(sourceFileInfo.FullName);
                        var destinationContents = File.ReadAllBytes(destinationFileInfo.FullName);
                        if (sourceContents.Length == destinationContents.Length) {
                            var differences = sourceContents.Where((t, i) => t != destinationContents[i]).Count();
                            if (differences == 0) {
                                continue;
                            }
                            if (folderUpdateMethod == FolderUpdateMethod.AssembliesButNotIfOnlySlightlyChanged && IsBinary(sourceFileInfo.Name) && differences < 50 && sourceFileInfo.Length >= MinimumBinaryFileSizeInBytes) {
                                continue;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(destinationFileInfo.DirectoryName) && !Directory.Exists(destinationFileInfo.DirectoryName)) {
                    Directory.CreateDirectory(destinationFileInfo.DirectoryName);
                }

                try {
                    File.Copy(sourceFileInfo.FullName, destinationFileInfo.FullName, true);
                } catch {
                    if (File.Exists(destinationFileInfo.FullName)) {
                        var newNameForFileToBeOverwritten = NewNameForFileToBeOverwritten(destinationFileInfo.DirectoryName, destinationFileInfo.Name);
                        try {
                            File.Move(destinationFileInfo.FullName, newNameForFileToBeOverwritten);
                        } catch {
                            errorsAndInfos.Errors.Add(string.Format(Properties.Resources.FailedToRename, destinationFileInfo.Name, newNameForFileToBeOverwritten.Substring(newNameForFileToBeOverwritten.LastIndexOf('\\') + 1)));
                            continue;
                        }
                    }

                    try {
                        File.Copy(sourceFileInfo.FullName, destinationFileInfo.FullName, true);
                    } catch {
                        errorsAndInfos.Errors.Add(string.Format(Properties.Resources.FailedToCopy, sourceFileInfo.FullName, destinationFileInfo.FullName));
                        continue;
                    }
                }

                File.SetLastWriteTime(destinationFileInfo.FullName, sourceFileInfo.LastWriteTime);
            }
        }

        protected static bool IsBinary(string fileName) {
            return fileName.EndsWith(@".exe") || fileName.EndsWith(@".dll") || fileName.EndsWith(@".pdb");
        }

        private static string NewNameForFileToBeOverwritten(string folder, string name) {
            uint n = 0;
            string newOriginalFileName;
            do {
                n++;
                newOriginalFileName = folder + @"\~" + n + '~' + name;
            } while (File.Exists(newOriginalFileName));

            return newOriginalFileName;
        }
    }
}
