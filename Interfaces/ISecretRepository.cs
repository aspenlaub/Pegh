namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ISecretRepository {
        void Set<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>, new();
        TResult Get<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>, new();
        TResult Get<TArgument, TResult>(ISecret<IPowershellFunction<TArgument, TResult>> secret, TArgument arg) where TResult : class;
        void Reset(IGuid secret);
        bool Exists(IGuid secret);
    }
}
