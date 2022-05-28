using System.Collections.Generic;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface ICsLambda {
    List<string> Namespaces { get; }
    List<string> Types { get; }
    string LambdaExpression { get; }
}