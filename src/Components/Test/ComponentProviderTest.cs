using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class ComponentProviderTest {
        [TestMethod]
        public void ComponentsAreProvided() {
            var sut = new ComponentProvider();
            Assert.IsNotNull(sut.Disguiser);
            Assert.IsNotNull(sut.FolderDeleter);
            Assert.IsNotNull(sut.FolderUpdater);
            Assert.IsNotNull(sut.PassphraseProvider);
            Assert.IsNotNull(sut.PeghEnvironment);
            Assert.IsNotNull(sut.PrimeNumberGenerator);
            Assert.IsNotNull(sut.SecretRepository);
            Assert.IsNotNull(sut.StringCrypter);
            Assert.IsNotNull(sut.XmlDeserializer);
            Assert.IsNotNull(sut.XmlSerializer);
            Assert.IsNotNull(sut.XmlSchemer);
        }
    }
}
