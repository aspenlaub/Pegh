namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ISecret<TResult> : IGuid {
        SecretTypes SecretType { get; }
        TResult Value { get; set; }
        TResult DefaultValue { get; }
    }
}
