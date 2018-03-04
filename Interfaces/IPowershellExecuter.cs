namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IPowershellExecuter {
        TResult ExecutePowershellFunction<TArgument, TResult>(IPowershellFunction<TArgument, TResult> powershellFunction, TArgument arg) where TResult : class;
        bool ExecutePowershellFunction(string powershellScriptContents);
    }
}
