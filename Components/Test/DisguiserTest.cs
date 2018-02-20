﻿using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    [TestClass]
    public class DisguiserTest {
        [TestMethod]
        public void CanDisguise() {
            var componentProviderMock = new Mock<IComponentProvider>();

            var secretLongString = new LongString {TheLongString = "A12BCD3456./*EFG78HI"};

            var secretRepositoryMock = new Mock<ISecretRepository>();
            secretRepositoryMock.Setup(repository => repository.Get(It.IsAny<LongSecretString>())).Returns(secretLongString);
            componentProviderMock.Setup(c => c.SecretRepository).Returns(secretRepositoryMock.Object);

            componentProviderMock.Setup(c => c.PrimeNumberGenerator).Returns(new PrimeNumberGenerator());

            var sut = new Disguiser(componentProviderMock.Object);
            var s = "This is a test string";
            var disguised = sut.Disguise(s);
            Assert.IsTrue(disguised.Length >= 3 * s.Length);
            Assert.IsFalse(disguised.Contains(s));
            Assert.AreEqual("./*56.D34CD3456/*E./*56.456./*FG7BCD78HBCD456*EF345BCDD342BC/*E", disguised);

            s = "Short string";
            disguised = sut.Disguise(s);
            Assert.IsTrue(disguised.Length >= 3 * s.Length);
            Assert.IsFalse(disguised.Contains(s));
        }
    }
}