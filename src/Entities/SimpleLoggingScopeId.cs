using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class SimpleLoggingScopeId : ISimpleLoggingScopeId {
    public string ClassOrMethod { get; init; }
    public string Id { get; init; }

    public static ISimpleLoggingScopeId Create(string classOrMethodName) {
        var id = Guid.NewGuid().ToString();
        id = id.Substring(0, id.IndexOf('-'));
        id = DateTime.Now.Second.ToString("D2") + DateTime.Now.Millisecond.ToString("D3") + id;
        return new SimpleLoggingScopeId { ClassOrMethod = classOrMethodName, Id = id };
    }
}