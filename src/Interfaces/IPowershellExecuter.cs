using System.Collections.Generic;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IPowershellExecuter {
        TResult ExecutePowershellFunction<TArgument, TResult>(IPowershellFunction<TArgument, TResult> powershellFunction, TArgument arg) where TResult : class;
        void ExecutePowershellScriptFile(string powershellScriptFileName, out IList<string> errors);
    }
}
