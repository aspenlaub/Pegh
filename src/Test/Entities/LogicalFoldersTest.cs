using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities {
    [TestClass]
    public class LogicalFoldersTest {
        private static IContainer Container { get; set; }

        public LogicalFoldersTest() {
            var builder = new ContainerBuilder().RegisterForPeghTest();
            Container = builder.Build();
        }

        [TestMethod]
        public async Task CanGetLogicalFolders() {
            var secretRepository = Container.Resolve<ISecretRepository>();
            var logicalFoldersSecret = new LogicalFoldersSecret();
            var errorsAndInfos = new ErrorsAndInfos();
            var logicalFolders = await secretRepository.GetAsync(logicalFoldersSecret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
            Assert.IsTrue(logicalFolders.Any(m => m.Name == "MainUserFolder"));
        }
    }
}
