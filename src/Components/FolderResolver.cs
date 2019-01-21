using System;
using System.Collections.Generic;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class FolderResolver : IFolderResolver {
        protected IComponentProvider ComponentProvider;
        protected IDictionary<string, string> Replacements;

        public FolderResolver(IComponentProvider componentProvider) {
            ComponentProvider = componentProvider;
            Replacements = new Dictionary<string, string>();

            var secretRepository = componentProvider.SecretRepository;
            var errorsAndInfos = new ErrorsAndInfos();
            var machineDrivesSecret = new MachineDrivesSecret();
            var machineDrives = secretRepository.GetAsync(machineDrivesSecret, errorsAndInfos).Result;
            if (errorsAndInfos.AnyErrors()) {
                throw new Exception(errorsAndInfos.ErrorsToString());
            }
            machineDrives.DrivesOnThisMachine().ToList().ForEach(d => AddReplacement(d));

            var logicalFoldersSecret = new LogicalFoldersSecret();
            var logicalFolders = secretRepository.GetAsync(logicalFoldersSecret, errorsAndInfos).Result;
            if (errorsAndInfos.AnyErrors()) {
                throw new Exception(errorsAndInfos.ErrorsToString());
            }
            logicalFolders.ForEach(l => AddReplacement(l));

            var keys = Replacements.Keys.ToList();
            foreach (var key in keys) {
                Replacements[key] = ResolveIterative(Replacements[key]).FullName;
            }
        }

        private void AddReplacement(MachineDrive machineDrive) {
            var key = "$(" + machineDrive.Name + ")";
            var value = (machineDrive.Drive.ToUpper() + "C")[0] + ":";
            if (Replacements.ContainsKey(key)) {
                throw new Exception($"Machine drive {key} is already mapped");
            }
            Replacements.Add(key, value);
        }

        private void AddReplacement(LogicalFolder logicalFolder) {
            var key = "$(" + logicalFolder.Name + ")";
            var value = logicalFolder.Folder;
            if (Replacements.ContainsKey(key)) {
                throw new Exception($"Logical folder {key} is already mapped");
            }
            Replacements.Add(key, value);
        }

        private IFolder ResolveIterative(string folderToResolve) {
            string oldFolderToResolve;
            do {
                oldFolderToResolve = folderToResolve;
                foreach (var replacement in Replacements.Where(replacement => folderToResolve.Contains(replacement.Key))) {
                    folderToResolve = folderToResolve.Replace(replacement.Key, replacement.Value);
                }
            } while (oldFolderToResolve != folderToResolve);
            return new Folder(folderToResolve);
        }

        public IFolder Resolve(string folderToResolve) {
            foreach (var replacement in Replacements.Where(replacement => folderToResolve.Contains(replacement.Key))) {
                folderToResolve = folderToResolve.Replace(replacement.Key, replacement.Value);
            }
            return new Folder(folderToResolve);
        }
    }
}
