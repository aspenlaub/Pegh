using System.Collections.Generic;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class PrimeNumberGenerator : IPrimeNumberGenerator {
        public IEnumerable<int> Generate(int n) {
            var result = new List<int> { 2 };
            var candidate = 2;
            while (result.Count < n) {
                candidate ++;
                if (result.Any(j => candidate % j == 0)) { continue; }

                result.Add(candidate);
            }

            return result;
        }
    }
}
