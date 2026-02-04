using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class DummyCsLambdaCompiler : ICsLambdaCompiler {
    public Task<Func<TArgument, TResult>> CompileCsLambdaAsync<TArgument, TResult>(ICsLambda csLambda) {
        throw new NotSupportedException(nameof(ICsLambdaCompiler));
    }
}
