namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ISecretRepository {
        void Set<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>;
        void Get<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>;
        TResult Get<TArgument, TResult>(IPowershellSecret<TArgument, TResult> secret, TArgument arg) where TResult : class;
        void Reset<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>;
        bool Exists<TResult>(ISecret<TResult> secret) where TResult : class, ISecretResult<TResult>;
    }
}
