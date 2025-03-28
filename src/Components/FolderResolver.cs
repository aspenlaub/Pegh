﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class FolderResolver(ISecretRepository secretRepository) : IFolderResolver {
    protected readonly IDictionary<string, string> Replacements = new Dictionary<string, string>();

    protected readonly ISecretRepository SecretRepository = secretRepository;

    private async Task FindReplacementsIfNecessaryAsync() {
        if (Replacements.Any()) { return; }

        var errorsAndInfos = new ErrorsAndInfos();
        var machineDrivesSecret = new MachineDrivesSecret();
        var machineDrives = await SecretRepository.GetAsync(machineDrivesSecret, errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }
        machineDrives.DrivesOnThisMachine().ToList().ForEach(AddReplacement);

        var logicalFoldersSecret = new LogicalFoldersSecret();
        var logicalFolders = await SecretRepository.GetAsync(logicalFoldersSecret, errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }
        logicalFolders.ForEach(AddReplacement);

        var keys = Replacements.Keys.ToList();
        foreach (var key in keys) {
            Replacements[key] = ResolveIterative(Replacements[key]).FullName;
        }
    }

    private void AddReplacement(MachineDrive machineDrive) {
        var key = "$(" + machineDrive.Name + ")";
        var value = (machineDrive.Drive.ToUpper() + "C")[0] + ":";
        if (!Replacements.TryAdd(key, value)) {
            throw new Exception($"Machine drive {key} is already mapped");
        }
    }

    private void AddReplacement(LogicalFolder logicalFolder) {
        var key = "$(" + logicalFolder.Name + ")";
        var value = logicalFolder.Folder;
        if (!Replacements.TryAdd(key, value)) {
            throw new Exception($"Logical folder {key} is already mapped");
        }
    }

    private IFolder ResolveIterative(string folderToResolve) {
        string oldFolderToResolve;
        do {
            oldFolderToResolve = folderToResolve;
            foreach (var (key, value) in Replacements.Where(replacement => folderToResolve.Contains(replacement.Key))) {
                folderToResolve = folderToResolve.Replace(key, value);
            }
        } while (oldFolderToResolve != folderToResolve);
        return new Folder(folderToResolve);
    }

    public async Task<IFolder> ResolveAsync(string folderToResolve, IErrorsAndInfos errorsAndInfos) {
        if (folderToResolve == null) { return null; }

        await FindReplacementsIfNecessaryAsync();

        foreach (var (key, value) in Replacements.Where(replacement => folderToResolve.Contains(replacement.Key))) {
            folderToResolve = folderToResolve.Replace(key, value);
        }

        if (!folderToResolve.Contains("$(")) { return new Folder(folderToResolve); }

        var startIndex = -1;
        int endIndex;
        var machineDriveSecretFileName = SecretRepository.FileName(new MachineDrivesSecret(), false);
        var logicalFolderSecretFileName = SecretRepository.FileName(new LogicalFoldersSecret(), false);
        var missingPlaceHolders = new List<string>();
        do {
            startIndex = folderToResolve.IndexOf("$(", startIndex + 1, StringComparison.Ordinal);
            if (startIndex >= 0) {
                endIndex = folderToResolve.IndexOf(")", startIndex, StringComparison.Ordinal);
                missingPlaceHolders.Add("'" + folderToResolve.Substring(startIndex, endIndex - startIndex + 1) + "'");
            } else {
                endIndex = -1;

            }
        } while (startIndex >= 0 && endIndex > startIndex);

        errorsAndInfos.Errors.Add(missingPlaceHolders.Count == 1
            ? string.Format(Properties.Resources.FolderPlaceholderNotFound, missingPlaceHolders[0], machineDriveSecretFileName, logicalFolderSecretFileName)
            : string.Format(Properties.Resources.FolderPlaceholdersNotFound, string.Join(", ", missingPlaceHolders), machineDriveSecretFileName, logicalFolderSecretFileName));
        return new Folder(folderToResolve);
    }
}