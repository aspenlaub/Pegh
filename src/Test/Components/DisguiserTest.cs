using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Seoa.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Entities;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Interfaces;
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

        ContainerBuilder builder = new ContainerBuilder().UseForPeghTest(secretRepositoryMock.Object);
        Container = builder.Build();
    }

    [TestMethod]
    public async Task CanDisguise() {
        IDisguiser sut = Container.Resolve<IDisguiser>();
        ISecretRepository secretRepository = Container.Resolve<ISecretRepository>();
        string s = "This is a test string";
        var errorsAndInfos = new ErrorsAndInfos();
        string disguised = await sut.Disguise(secretRepository, s, errorsAndInfos);
        Assert.That.ThereWereNoErrors(errorsAndInfos);
        Assert.IsGreaterThanOrEqualTo(3 * s.Length, disguised.Length);
        Assert.DoesNotContain(s, disguised);
        Assert.AreEqual("./*56.D34CD3456/*E./*56.456./*FG7BCD78HBCD456*EF345BCDD342BC/*E", disguised);

        s = "Short string";
        disguised = await sut.Disguise(secretRepository, s, errorsAndInfos);
        Assert.That.ThereWereNoErrors(errorsAndInfos);
        Assert.IsGreaterThanOrEqualTo(3 * s.Length, disguised.Length);
        Assert.DoesNotContain(s, disguised);
    }
}