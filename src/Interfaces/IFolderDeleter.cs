// ReSharper disable UnusedMember.Global
namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface IFolderDeleter {
    /// <summary>
    /// Is it allowed to delete the folder?
    /// </summary>
    /// <param name="folder">The folder that shall be deleted</param>
    /// <returns>true if the folder may be deleted</returns>
    bool CanDeleteFolder(IFolder folder);

    /// <summary>
    /// Is it allowed to delete the folder?
    /// </summary>
    /// <param name="folder">The folder that shall be deleted</param>
    /// <param name="folderDeleteGates">Characteristics of the folder that led to the result</param>
    /// <returns>true if the folder may be deleted</returns>
    // ReSharper disable once UnusedMemberInSuper.Global
    bool CanDeleteFolder(IFolder folder, out IFolderDeleteGates folderDeleteGates);

    /// <summary>
    /// Double-check if it is allowed to delete the folder
    /// </summary>
    /// <param name="folder">The folder that shall be deleted</param>
    /// <returns>true if the folder may be deleted</returns>
    bool CanDeleteFolderDoubleCheck(IFolder folder);

    /// <summary>
    /// Delete the folder
    /// </summary>
    /// <param name="folder">The folder that shall be deleted</param>
    void DeleteFolder(IFolder folder);
}