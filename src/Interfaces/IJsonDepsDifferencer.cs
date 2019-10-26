namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IJsonDepsDifferencer {
        bool AreJsonDependenciesIdenticalExceptForNamespaceVersion(string oldJson, string newJson, string mainNamespace);
    }
}
