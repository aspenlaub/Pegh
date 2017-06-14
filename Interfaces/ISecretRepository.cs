namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ISecretRepository {
        void Set<TResult>(ISecret<TResult> secret);
        void Get<TResult>(ISecret<TResult> secret);
        void Get<TArgument, TResult>(ISecret<TResult> secret, TArgument arg);
        void Reset<TResult>(ISecret<TResult> secret);
        bool Exists<TResult>(ISecret<TResult> secret);
    }
}
