namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IFolderUpdater {
        void UpdateFolder(IFolder sourceFolder, IFolder destinationFolder, FolderUpdateMethod folderUpdateMethod, IErrorsAndInfos errorsAndInfos);
        void UpdateFolder(IFolder sourceFolder, IFolder destinationFolder, FolderUpdateMethod folderUpdateMethod, string mainNamespace, IErrorsAndInfos errorsAndInfos);
    }

    public enum FolderUpdateMethod {
        AssembliesButNotIfOnlySlightlyChanged = 1, AssembliesEvenIfOnlySlightlyChanged = 2
    }
}
