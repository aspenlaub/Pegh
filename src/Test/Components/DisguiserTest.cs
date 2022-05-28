using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable StringLiteralTypo

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class DisguiserTest {
    private static IContainer Container { get; set; }

    public DisguiserTest() {
        var secretLongString = new LongString { TheLongString = "A12BCD3456./*EFG78HI" };

        var secretRepositoryMock = new Mock<ISecretRepository>();
        secretRepositoryMock.Setup(repository => repository.GetAsync(It.IsAny<LongSecretString>(), It.IsAny<IErrorsAndInfos>())).Returns(Task.FromResult(secretLongString));

        var builder = new ContainerBuilder().UseForPeghTest(secretRepositoryMock.Object);
        Container = builder.Build();
    }

    [TestMethod]
    public async Task CanDisguise() {
        var sut = Container.Resolve<IDisguiser>();
        var secretRepository = Container.Resolve<ISecretRepository>();
        var s = "This is a test string";
        var errorsAndInfos = new ErrorsAndInfos();
        var disguised = await sut.Disguise(secretRepository, s, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        Assert.IsTrue(disguised.Length >= 3 * s.Length);
        Assert.IsFalse(disguised.Contains(s));
        Assert.AreEqual("./*56.D34CD3456/*E./*56.456./*FG7BCD78HBCD456*EF345BCDD342BC/*E", disguised);

        s = "Short string";
        disguised = await sut.Disguise(secretRepository, s, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsToString());
        Assert.IsTrue(disguised.Length >= 3 * s.Length);
        Assert.IsFalse(disguised.Contains(s));
    }
}