using System.Collections.Generic;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ICsScript {
        List<CsScriptArgument> StringArgumentNameToDescriptions { get; }
        string Script { get; }
        int TimeoutInSeconds { get; }
    }
}
