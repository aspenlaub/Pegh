namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface IFolderDeleteGates {
    /// <summary>
    /// Is the folder name long enough to allow deletion?
    /// </summary>
    bool FolderNameIsLongEnough { get; }

    /// <summary>
    /// Does the folder end with \obj, i.e. is it an intermediate output folder?
    /// </summary>
    bool EndsWithObj { get; }

    /// <summary>
    /// Does the folder end with \bin, i.e. is it an output folder?
    /// </summary>
    bool EndsWithBin { get; }

    /// <summary>
    /// Does the folder hold a number of files small enough to allow deletion?
    /// </summary>
    bool NotTooManyFilesInFolder { get; set; }

    /// <summary>
    /// Is the folder located beneath C:\Temp\?
    /// </summary>
    bool CTemp { get; }

    /// <summary>
    /// Does the folder originate from a git checkout?
    /// </summary>
    bool IsGitCheckOutFolder { get; }

    /// <summary>
    /// Is the folder located beneath the users temp folder (%TEMP%)?
    /// </summary>
    bool UserTemp { get; }

    /// <summary>
    /// Does the folder contain test results?
    /// </summary>
    bool TestResults { get; }
}