using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities {
    [TestClass]
    public class LogicalFoldersTest {
        [TestMethod]
        public async Task CanGetLogicalFolders() {
            var componentProvider = new ComponentProvider();
            var secretRepository = componentProvider.SecretRepository;
            var logicalFoldersSecret = new LogicalFoldersSecret();
            var errorsAndInfos = new ErrorsAndInfos();
            var logicalFolders = await secretRepository.GetAsync(logicalFoldersSecret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
            Assert.IsTrue(logicalFolders.Any(m => m.Name == "MainUserFolder"));
        }
    }
}
