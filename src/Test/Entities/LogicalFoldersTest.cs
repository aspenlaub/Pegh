using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities;

[TestClass]
public class LogicalFoldersTest {
    private static IContainer Container { get; set; }

    public LogicalFoldersTest() {
        ContainerBuilder builder = new ContainerBuilder().UseForPeghTest();
        Container = builder.Build();
    }

    [TestMethod]
    public async Task CanGetLogicalFolders() {
        ISecretRepository secretRepository = Container.Resolve<ISecretRepository>();
        var logicalFoldersSecret = new LogicalFoldersSecret();
        var errorsAndInfos = new ErrorsAndInfos();
        LogicalFolders logicalFolders = await secretRepository.GetAsync(logicalFoldersSecret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        Assert.Contains(m => m.Name == "MainUserFolder", logicalFolders);
    }
}