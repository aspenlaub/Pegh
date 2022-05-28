using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;

public static class FolderExtensions {
    public static bool Exists(this IFolder folder) {
        return Directory.Exists(folder.FullName);
    }

    public static bool HasSubFolder(this IFolder folder, string subFolder) {
        return SubFolder(folder, subFolder).Exists();
    }

    public static IFolder ParentFolder(this IFolder folder) {
        return !folder.FullName.Contains("\\") ? null : new Folder(folder.FullName[..folder.FullName.LastIndexOf('\\')]);
    }

    public static IFolder SubFolder(this IFolder folder, string subFolder) {
        return new Folder(folder.FullName + (subFolder.StartsWith("\\") ? subFolder : "\\" + subFolder));
    }

    // ReSharper disable once UnusedMember.Global
    public static string LastWrittenFileFullName(this IFolder folder) {
        return Directory.GetFiles(folder.FullName, "*.*").MaxBy(f => File.GetLastWriteTime(f));
    }

    public static IFolder GitSubFolder(this IFolder folder) {
        return folder.SubFolder(".git");
    }

    public static void CreateIfNecessary(this IFolder folder) {
        if (Directory.Exists(folder.FullName)) { return; }

        Directory.CreateDirectory(folder.FullName);
    }

    public static void DeleteLinks(this IFolder folder) {
        var directories = Directory.GetDirectories(folder.FullName, "*", SearchOption.TopDirectoryOnly)
            .Select(d => new DirectoryInfo(d))
            .Where(d => (d.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
            .ToList();
        foreach (var directory in directories) {
            directory.Delete();
        }
        var files = Directory.GetFiles(folder.FullName, "*.lnk", SearchOption.TopDirectoryOnly).ToList();
        foreach (var file in files) {
            File.Delete(file);
        }
    }
}