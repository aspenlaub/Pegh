using System.IO;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

public class FakeExceptionFolderProvider : IExceptionFolderProvider {
    public IFolder ExceptionFolder() {
        var folder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubExceptions").SubFolder(nameof(SimpleLoggerTest));
        folder.CreateIfNecessary();
        return folder;
    }
}