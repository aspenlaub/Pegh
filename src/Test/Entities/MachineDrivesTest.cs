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
        Assert.IsTrue(machineDrives.Any(m => m.Name == "CSharpDrive"));
        var drivesOnThisMachine = machineDrives.DrivesOnThisMachine();
        Assert.AreEqual(1, drivesOnThisMachine.Count(m => m.Name == "CSharpDrive"));
    }
}