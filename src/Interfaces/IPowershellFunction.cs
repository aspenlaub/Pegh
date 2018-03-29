namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IPowershellFunction<TArgument, TResult> : IGuid, ISecretResult<IPowershellFunction<TArgument, TResult>> {
        string FunctionName { get; }
        string Script { get; }
        string ArgumentType { get; }
        string ResultType { get; }
    }
}
