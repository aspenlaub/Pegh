using System;
using System.IO;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class AssemblyRepository : IAssemblyRepository {
        public string Folder { get; }

        public AssemblyRepository(IComponentProvider components) {
            Folder = components.PeghEnvironment.RootWorkFolder + @"\AssemblyRepository";
            components.FolderHelper.CreateIfNecessary(Folder);
        }

        public void AddToRepositoryIfNecessary(Type t) {
            var sourceFileFullName = t.Assembly.Location;

            AddToRepositoryIfNecessary(sourceFileFullName, ".dll");
            AddToRepositoryIfNecessary(sourceFileFullName, ".pdb");
        }

        public void AddToRepositoryIfNecessary(string sourceFileFullName, string ending) {
            if (sourceFileFullName == null) { return; }

            if (!string.IsNullOrEmpty(ending)) {
                sourceFileFullName = sourceFileFullName.Replace(".dll", ending);
            }
            if (sourceFileFullName.Contains(@"\Debug\")  && !sourceFileFullName.EndsWith(".Test.dll")) { return; }

            var shortName = sourceFileFullName.Substring(sourceFileFullName.LastIndexOf('\\') + 1);
            var destinationFileFullName = Folder + '\\' + shortName;
            if (File.Exists(destinationFileFullName) && File.GetLastWriteTime(sourceFileFullName) == File.GetLastWriteTime(destinationFileFullName)) { return; }

            File.Copy(sourceFileFullName, destinationFileFullName, true);
            File.SetLastAccessTime(destinationFileFullName, File.GetLastWriteTime(sourceFileFullName));
        }

        public void UpdateIncludeFolder(string sourceFileFullName, out bool success, out int numberOfRepositoryFiles, out int numberOfCopiedFiles) {
            success = false;
            numberOfRepositoryFiles = 0;
            numberOfCopiedFiles = 0;
            var folder = sourceFileFullName.Substring(0, sourceFileFullName.LastIndexOf('\\')) + @"\Include";
            if (!Directory.Exists(folder)) { return; }

            foreach (var destinationFileFullName in Directory.GetFiles(folder)) {
                numberOfRepositoryFiles ++;
                var shortFileName = destinationFileFullName.Substring(destinationFileFullName.LastIndexOf('\\') + 1);
                var repositoryFileFullName = Folder + '\\' + shortFileName;
                if (!File.Exists(repositoryFileFullName)) { continue; }
                if (File.GetLastWriteTime(destinationFileFullName) == File.GetLastWriteTime(repositoryFileFullName)) { continue; }

                File.Copy(repositoryFileFullName, destinationFileFullName, true);
                File.SetLastWriteTime(destinationFileFullName, File.GetLastWriteTime(repositoryFileFullName));
                numberOfCopiedFiles ++;
            }

            success = true;
        }
    }
}
