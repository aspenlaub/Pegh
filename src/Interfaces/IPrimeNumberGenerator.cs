using System.Collections.Generic;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface IPrimeNumberGenerator {
    IEnumerable<int> Generate(int n);
}