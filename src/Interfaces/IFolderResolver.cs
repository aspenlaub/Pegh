using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IFolderResolver {
        /// <summary>
        /// Resolve placeholders in folderToResolve
        /// Two kinds of secret placeholders exist: MachineDrivesSecret and LogicalFoldersSecret
        /// </summary>
        /// <param name="folderToResolve"></param>
        /// <param name="errorsAndInfos"></param>
        /// <returns>The resolved folder</returns>
        Task<IFolder> ResolveAsync(string folderToResolve, IErrorsAndInfos errorsAndInfos);
    }
}