using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class LogicalFoldersSecret : ISecret<LogicalFolders> {
    private LogicalFolders _LogicalFolders;
    public LogicalFolders DefaultValue => _LogicalFolders ??= new LogicalFolders {
        new() { Name = "FolderTest", Folder = @"c:\temp\folder\test" }
    };

    public string Guid => "724FAF2F-327F-4134-A7A7-EC350EF51DC4";
}