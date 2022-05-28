namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface ISecretResult<out T> where T : class {
    T Clone();
}