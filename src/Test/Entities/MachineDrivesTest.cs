using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities {
    [TestClass]
    public class MachineDrivesTest {
        [TestMethod]
        public async Task CanGetMachineDrives() {
            var componentProvider = new ComponentProvider();
            var secretRepository = componentProvider.SecretRepository;
            var machineDrivesSecret = new MachineDrivesSecret();
            var errorsAndInfos = new ErrorsAndInfos();
            var machineDrives = await secretRepository.GetAsync(machineDrivesSecret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
            Assert.IsTrue(machineDrives.Any(m => m.Name == "CSharpDrive"));
            var drivesOnThisMachine = machineDrives.DrivesOnThisMachine();
            Assert.AreEqual(1, drivesOnThisMachine.Count(m => m.Name == "CSharpDrive"));
        }
    }
}
