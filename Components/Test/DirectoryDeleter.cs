using System.IO;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    internal class DirectoryDeleter {
        internal static void DeleteDirectory(string folder) {
            if (!folder.Contains("Temp")) {
                return;
            }

            if (!Directory.Exists(folder)) {
                return;
            }

            foreach (var file in Directory.GetFiles(folder)) {
                File.Delete(file);
            }

            foreach (var directory in Directory.GetDirectories(folder)) {
                DeleteDirectory(directory);
            }

            Directory.Delete(folder);
        }
    }
}
