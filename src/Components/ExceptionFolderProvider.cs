using System.IO;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class ExceptionFolderProvider : IExceptionFolderProvider {
    public IFolder ExceptionFolder() {
        var folder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubExceptions").SubFolder(nameof(SimpleLogger));
        folder.CreateIfNecessary();
        return folder;
    }
}