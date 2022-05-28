using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class FolderDeleteGates : IFolderDeleteGates {
    public bool FolderNameIsLongEnough { get; init; }
    public bool EndsWithObj { get; init; }
    public bool EndsWithBin { get; init; }
    public bool NotTooManyFilesInFolder { get; set; }
    public bool CTemp { get; init; }
    public bool IsGitCheckOutFolder { get; init; }
    public bool UserTemp { get; init; }
    public bool TestResults { get; init; }
}