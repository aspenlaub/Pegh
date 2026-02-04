using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class StringCrypterTest {
    private static IContainer Container { get; set; }

    public StringCrypterTest() {
        ContainerBuilder builder = new ContainerBuilder().UseForPeghTest();
        Container = builder.Build();
    }

    [TestMethod]
    public async Task CanEncryptAndDecryptString() {
        IStringCrypter sut = Container.Resolve<IStringCrypter>();
        const string s = "2018-10-3076MuWwlgbBtCHxwW";
        string encrypted = await sut.EncryptAsync(s);
        string decrypted = await sut.DecryptAsync(encrypted);
        Assert.AreEqual(s, decrypted);
    }
}