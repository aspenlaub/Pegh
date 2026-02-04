using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities;

[TestClass]
public class MachineDrivesTest {
    private static IContainer Container { get; set; }

    public MachineDrivesTest() {
        var builder = new ContainerBuilder().UseForPeghTest();
        Container = builder.Build();
    }

    [TestMethod]
    public async Task CanGetMachineDrives() {
        var secretRepository = Container.Resolve<ISecretRepository>();
        var machineDrivesSecret = new MachineDrivesSecret();
        var errorsAndInfos = new ErrorsAndInfos();
        var machineDrives = await secretRepository.GetAsync(machineDrivesSecret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        Assert.Contains(m => m.Name == "CSharpDrive", machineDrives);
        var drivesOnThisMachine = machineDrives.DrivesOnThisMachine();
        Assert.ContainsSingle(m => m.Name == "CSharpDrive", drivesOnThisMachine);
    }
}