﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class Disguiser : IDisguiser {
        protected IComponentProvider ComponentProvider;
        protected ISecretRepository SecretRepository;
        protected IPrimeNumberGenerator PrimeNumberGenerator;
        protected IList<int> PrimeNumbers;

        public Disguiser(IComponentProvider componentProvider) {
            ComponentProvider = componentProvider;
            SecretRepository = componentProvider.SecretRepository;
            PrimeNumberGenerator = ComponentProvider.PrimeNumberGenerator;
        }

        protected async Task<string> LongString(IErrorsAndInfos errorsAndInfos) {
            var secretLongString = await SecretRepository.GetAsync(new LongSecretString(), errorsAndInfos);
            return secretLongString.TheLongString;
        }

        public async Task<string> Disguise(string s, IErrorsAndInfos errorsAndInfos) {
            var bytes = Encoding.UTF8.GetBytes(s);
            EnsurePrimeNumbers(bytes);
            long pos = bytes.Length;
            var primePos = bytes.Length;
            var disguised = "";
            var longString = await LongString(errorsAndInfos);
            foreach (var aByte in bytes) {
                pos = pos + aByte * PrimeNumbers[primePos];
                primePos = primePos + aByte;
                disguised = disguised + longString.Substring((int)(pos % (longString.Length - 3)), 3);
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
