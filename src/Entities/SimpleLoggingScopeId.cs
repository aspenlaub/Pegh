using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class SimpleLoggingScopeId : ISimpleLoggingScopeId {
    public string ClassOrMethod { get; init; }
    public string Id { get; init; }

    public static ISimpleLoggingScopeId Create(string classOrMethodName, string id) {
        return new SimpleLoggingScopeId { ClassOrMethod = classOrMethodName, Id = id };
    }
}