﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class Disguiser(IPrimeNumberGenerator primeNumberGenerator) : IDisguiser {
    protected readonly IPrimeNumberGenerator PrimeNumberGenerator = primeNumberGenerator;
    protected IList<int> PrimeNumbers;

    public async Task<string> Disguise(ISecretRepository secretRepository, string s, IErrorsAndInfos errorsAndInfos) {
        var bytes = Encoding.UTF8.GetBytes(s);
        EnsurePrimeNumbers(bytes);
        long pos = bytes.Length;
        var primePos = bytes.Length;
        var disguised = "";
        var secretLongString = await secretRepository.GetAsync(new LongSecretString(), errorsAndInfos);
        var longString = secretLongString.TheLongString;
        foreach (var aByte in bytes) {
            pos += aByte * PrimeNumbers[primePos];
            primePos += aByte;
            disguised += longString.Substring((int)(pos % (longString.Length - 3)), 3);
        }

        return disguised;
    }

    protected void EnsurePrimeNumbers(byte[] bytes) {
        var needed = (bytes.Length  + 1) * 256;
        if (PrimeNumbers?.Count >= needed) { return; }

        PrimeNumbers = PrimeNumberGenerator.Generate(needed).ToList();
    }
}