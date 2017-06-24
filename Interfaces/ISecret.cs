namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ISecret<out TResult> : IGuid where TResult : class, ISecretResult<TResult> {
        TResult DefaultValue { get; }
    }
}
