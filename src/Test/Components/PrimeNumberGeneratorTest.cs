using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

[TestClass]
public class PrimeNumberGeneratorTest {
    [TestMethod]
    public void CanGenerateFirstPrimeNumbers() {
        var sut = new PrimeNumberGenerator();
        var primeNumbers = sut.Generate(5).ToList();
        var expectedPrimeNumbers = new[] {2, 3, 5, 7, 11};
        Assert.HasCount(expectedPrimeNumbers.Length, primeNumbers);
        for (var i = 0; i < primeNumbers.Count; i++) {
            Assert.AreEqual(expectedPrimeNumbers[i], primeNumbers[i]);
        }
    }

    [TestMethod]
    public void PickedPrimeNumbersAreAsExpected() {
        var sut = new PrimeNumberGenerator();
        var primeNumbers = sut.Generate(15).ToList();
        Assert.AreEqual(47, primeNumbers[14]);
        var otherPrimeNumbers = sut.Generate(155).ToList();
        Assert.AreEqual(907, otherPrimeNumbers[154]);
    }

    [TestMethod]
    public void PrimeNumbersAreReturnedInSameSequence() {
        var sut = new PrimeNumberGenerator();
        var primeNumbers = sut.Generate(1155).ToList();
        var otherPrimeNumbers = sut.Generate(1515).ToList();
        for (var i = 0; i < primeNumbers.Count; i++) {
            Assert.AreEqual(primeNumbers[i], otherPrimeNumbers[i]);
        }
    }
}