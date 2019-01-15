using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable StringLiteralTypo

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    [TestClass]
    public class DisguiserTest {
        [TestMethod]
        public async Task CanDisguise() {
            var componentProviderMock = new Mock<IComponentProvider>();

            var secretLongString = new LongString {TheLongString = "A12BCD3456./*EFG78HI"};

            var secretRepositoryMock = new Mock<ISecretRepository>();
            secretRepositoryMock.Setup(repository => repository.GetAsync(It.IsAny<LongSecretString>(), It.IsAny<IErrorsAndInfos>())).Returns(Task.FromResult(secretLongString));
            componentProviderMock.Setup(c => c.SecretRepository).Returns(secretRepositoryMock.Object);

            componentProviderMock.Setup(c => c.PrimeNumberGenerator).Returns(new PrimeNumberGenerator());

            IDisguiser sut = new Disguiser(componentProviderMock.Object);
            var s = "This is a test string";
            var errorsAndInfos = new ErrorsAndInfos();
            var disguised = await sut.Disguise(s, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsTrue(disguised.Length >= 3 * s.Length);
            Assert.IsFalse(disguised.Contains(s));
            Assert.AreEqual("./*56.D34CD3456/*E./*56.456./*FG7BCD78HBCD456*EF345BCDD342BC/*E", disguised);

            s = "Short string";
            disguised = await sut.Disguise(s, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), string.Join("\r\n", errorsAndInfos.Errors));
            Assert.IsTrue(disguised.Length >= 3 * s.Length);
            Assert.IsFalse(disguised.Contains(s));
        }
    }
}
