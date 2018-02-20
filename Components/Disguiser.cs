﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class Disguiser : IDisguiser {
        protected IComponentProvider ComponentProvider;
        protected ISecretRepository SecretRepository;
        protected string LongString;
        protected IPrimeNumberGenerator PrimeNumberGenerator;
        protected IList<int> PrimeNumbers;

        public Disguiser(IComponentProvider componentProvider) {
            ComponentProvider = componentProvider;
            SecretRepository = componentProvider.SecretRepository;
            LongString = SecretRepository.Get(new LongSecretString()).TheLongString;
            PrimeNumberGenerator = ComponentProvider.PrimeNumberGenerator;
        }

        public string Disguise(string s) {
            var bytes = Encoding.UTF8.GetBytes(s);
            EnsurePrimeNumbers(bytes);
            long pos = bytes.Length;
            var primePos = bytes.Length;
            var disguised = "";
            foreach (var aByte in bytes) {
                pos = pos + aByte * PrimeNumbers[primePos];
                primePos = primePos + aByte;
                disguised = disguised + LongString.Substring((int)(pos % (LongString.Length - 3)), 3);
            }

            return disguised;
        }

        protected void EnsurePrimeNumbers(byte[] bytes) {
            var needed = (bytes.Length  + 1) * 256;
            if (PrimeNumbers != null && PrimeNumbers.Count >= needed) { return; }

            PrimeNumbers = PrimeNumberGenerator.Generate(needed).ToList();
        }
    }
}