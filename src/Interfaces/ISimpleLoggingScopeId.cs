namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface ISimpleLoggingScopeId {
    string ClassOrMethod { get; }
    string Id { get; }
}