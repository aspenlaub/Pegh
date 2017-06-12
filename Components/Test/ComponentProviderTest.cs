using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class ComponentProviderTest {
        [TestMethod]
        public void ComponentsAreProvided() {
            var sut = new ComponentProvider();
            Assert.IsNotNull(sut.AssemblyRepository);
            Assert.IsNotNull(sut.FolderHelper);
        }
    }
}
