using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    [TestClass]
    public class StringCrypterTest {
        private static IContainer Container { get; set; }

        public StringCrypterTest() {
            var builder = new ContainerBuilder().UseForPeghTest();
            Container = builder.Build();
        }

        [TestMethod]
        public void CanEncryptStringWithoutEscapingUnicodeCharacters() {
            var sut = Container.Resolve<IStringCrypter>();
            var encrypted = sut.Encrypt("2018-10-3076MuWwlgbBtCHxwW");
            Assert.IsFalse(encrypted.Contains("\\u"));
        }
    }
}
