using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public static class FolderExtensions {
        public static bool Exists(this IFolder folder) {
            return Directory.Exists(folder.FullName);
        }

        public static bool HasSubFolder(this IFolder folder, string subFolder) {
            return SubFolder(folder, subFolder).Exists();
        }

        public static IFolder ParentFolder(this IFolder folder) {
            return !folder.FullName.Contains("\\") ? null : new Folder(folder.FullName.Substring(0, folder.FullName.LastIndexOf('\\')));
        }

        public static IFolder SubFolder(this IFolder folder, string subFolder) {
            return new Folder(folder.FullName + (subFolder.StartsWith("\\") ? subFolder : "\\" + subFolder));
        }

        public static string LastWrittenFileFullName(this IFolder folder) {
            return Directory.GetFiles(folder.FullName, "*.*").OrderByDescending(f => File.GetLastWriteTime(f)).FirstOrDefault();
        }

        public static IFolder GitSubFolder(this IFolder folder) {
            return folder.SubFolder(".git");
        }

        public static void CreateIfNecessary(this IFolder folder) {
            if (Directory.Exists(folder.FullName)) { return; }

            Directory.CreateDirectory(folder.FullName);
        }

    }
}
