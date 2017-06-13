namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IComponentProvider {
        IAssemblyRepository AssemblyRepository { get; }
        IFolderHelper FolderHelper { get; }
        IPeghEnvironment PeghEnvironment { get; }
    }
}
