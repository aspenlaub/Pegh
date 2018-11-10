using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class CsLambdaCompiler : ICsLambdaCompiler {
        public async Task<Func<TArgument, TResult>> CompileCsLambdaAsync<TArgument, TResult>(ICsLambda csLambda) {
            var options = ScriptOptions.Default;
            if (csLambda.Namespaces.Any()) {
                options = options.AddImports(csLambda.Namespaces);
            }
            if (csLambda.Types.Any()) {
                options = options.AddReferences(csLambda.Types.Select(Type2Assembly));
            }

            options = options.AddReferences(typeof(TArgument).Assembly);
            options = options.AddReferences(typeof(TResult).Assembly);
            return await CSharpScript.EvaluateAsync<Func<TArgument, TResult>>(csLambda.LambdaExpression, options);
        }

        public Assembly Type2Assembly(string type) {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.FirstOrDefault(a => a.GetType(type, false) != null);
        }
    }
}
