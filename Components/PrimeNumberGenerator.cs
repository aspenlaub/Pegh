using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class PrimeNumberGenerator : IPrimeNumberGenerator {
        public IEnumerable<int> Generate(int n) {
            var result = new List<int> { 2 };
            var candidate = 2;
            while (result.Count < n) {
                candidate ++;
                var prime = true;
                for (var j = 2; j <= candidate / 2; j++) {
                    if (candidate % j != 0) {
                        continue;
                    }

                    prime = false;
                    break;
                }

                if (!prime) { continue; }

                result.Add(candidate);
            }

            return result;
        }
    }
}
