using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities;

[TestClass]
public class SecretStringCrypterFunctionTest {
    private static IContainer Container { get; set; }

    public SecretStringCrypterFunctionTest() {
        var builder = new ContainerBuilder().UseForPeghTest();
        Container = builder.Build();
    }

    [TestMethod]
    public async Task CanEncryptAndDecryptStrings() {
        var secretRepository = Container.Resolve<ISecretRepository>();

        var encrypterSecret = new SecretStringEncrypterFunction();
        Assert.IsFalse(string.IsNullOrEmpty(encrypterSecret.Guid));
        var secretEncrypterFunction = await secretRepository.CompileCsLambdaAsync<string, string>(encrypterSecret.DefaultValue);

        var decrypterSecret = new SecretStringDecrypterFunction();
        Assert.IsFalse(string.IsNullOrEmpty(decrypterSecret.Guid));
        var secretDecrypterFunction = await secretRepository.CompileCsLambdaAsync<string, string>(decrypterSecret.DefaultValue);

        const string originalString = "Whatever you do not want to reveal, keep it secret (\\, € ✂ and ❤)!";
        var encryptedString = secretEncrypterFunction(originalString);
        Assert.AreNotEqual(originalString, encryptedString);
        var decryptedString = secretDecrypterFunction(encryptedString);
        Assert.AreEqual(originalString, decryptedString);
    }

    [TestMethod]
    public async Task CanEncryptAndDecryptStringsUsingRealEncrypter() {
        var secretRepository = Container.Resolve<ISecretRepository>();

        var encrypterSecret = new SecretStringEncrypterFunction();
        Assert.IsFalse(string.IsNullOrEmpty(encrypterSecret.Guid));
        var errorsAndInfos = new ErrorsAndInfos();
        var csLambda = await secretRepository.GetAsync(encrypterSecret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        var secretEncrypterFunction = await secretRepository.CompileCsLambdaAsync<string, string>(csLambda);

        var decrypterSecret = new SecretStringDecrypterFunction();
        Assert.IsFalse(string.IsNullOrEmpty(decrypterSecret.Guid));
        csLambda = await secretRepository.GetAsync(decrypterSecret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        var secretDecrypterFunction = await secretRepository.CompileCsLambdaAsync<string, string>(csLambda);

        const string originalString = "Whatever you do not want to reveal, keep it secret (\\, € ✂ and ❤)!";

        var encryptedString = secretEncrypterFunction(originalString);
        Assert.AreNotEqual(originalString, encryptedString);

        var decryptedString = secretDecrypterFunction(encryptedString);
        Assert.AreEqual(originalString, decryptedString);
    }
}