namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IEncryptedSecret<out TResult> : ISecret<TResult> where TResult : class, ISecretResult<TResult> {
    }
}
