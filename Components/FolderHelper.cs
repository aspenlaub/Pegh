using System.IO;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class FolderHelper : IFolderHelper {
        public void CreateIfNecessary(string folder) {
            if (Directory.Exists(folder)) { return; }

            Directory.CreateDirectory(folder);
        }
    }
}
