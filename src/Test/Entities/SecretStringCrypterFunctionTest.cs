using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;
using Aspenlaub.Net.GitHub.CSharp.Seoa.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Entities;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Entities;

[TestClass]
public class SecretStringCrypterFunctionTest {
    private static IContainer Container { get; set; }

    public SecretStringCrypterFunctionTest() {
        ContainerBuilder builder = new ContainerBuilder().UseForPeghTest();
        Container = builder.Build();
    }

    [TestMethod]
    public async Task CanEncryptAndDecryptStrings() {
        ISecretRepository secretRepository = Container.Resolve<ISecretRepository>();

        var encrypterSecret = new SecretStringEncrypterFunction();
        Assert.IsFalse(string.IsNullOrEmpty(encrypterSecret.Guid));
        Func<string, string> secretEncrypterFunction = await secretRepository.CompileCsLambdaAsync<string, string>(encrypterSecret.DefaultValue);

        var decrypterSecret = new SecretStringDecrypterFunction();
        Assert.IsFalse(string.IsNullOrEmpty(decrypterSecret.Guid));
        Func<string, string> secretDecrypterFunction = await secretRepository.CompileCsLambdaAsync<string, string>(decrypterSecret.DefaultValue);

        const string originalString = "Whatever you do not want to reveal, keep it secret (\\, € ✂ and ❤)!";
        string encryptedString = secretEncrypterFunction(originalString);
        Assert.AreNotEqual(originalString, encryptedString);
        string decryptedString = secretDecrypterFunction(encryptedString);
        Assert.AreEqual(originalString, decryptedString);
    }

    [TestMethod]
    public async Task CanEncryptAndDecryptStringsUsingRealEncrypter() {
        ISecretRepository secretRepository = Container.Resolve<ISecretRepository>();

        var encrypterSecret = new SecretStringEncrypterFunction();
        Assert.IsFalse(string.IsNullOrEmpty(encrypterSecret.Guid));
        var errorsAndInfos = new ErrorsAndInfos();
        CsLambda csLambda = await secretRepository.GetAsync(encrypterSecret, errorsAndInfos);
        Assert.That.ThereWereNoErrors(errorsAndInfos);
        Func<string, string> secretEncrypterFunction = await secretRepository.CompileCsLambdaAsync<string, string>(csLambda);

        var decrypterSecret = new SecretStringDecrypterFunction();
        Assert.IsFalse(string.IsNullOrEmpty(decrypterSecret.Guid));
        csLambda = await secretRepository.GetAsync(decrypterSecret, errorsAndInfos);
        Assert.That.ThereWereNoErrors(errorsAndInfos);
        Func<string, string> secretDecrypterFunction = await secretRepository.CompileCsLambdaAsync<string, string>(csLambda);

        const string originalString = "Whatever you do not want to reveal, keep it secret (\\, € ✂ and ❤)!";

        string encryptedString = secretEncrypterFunction(originalString);
        Assert.AreNotEqual(originalString, encryptedString);

        string decryptedString = secretDecrypterFunction(encryptedString);
        Assert.AreEqual(originalString, decryptedString);
    }
}