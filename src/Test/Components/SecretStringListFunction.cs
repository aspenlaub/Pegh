using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    public class SecretStringListFunction : ISecret<CsScript> {
        private static CsScript vDefaultCsScript;
        public CsScript DefaultValue => vDefaultCsScript ?? (vDefaultCsScript = CreateDefaultCsScript());

        private static CsScript CreateDefaultCsScript() {
            var script = new CsScript(new List<CsScriptArgument> { new CsScriptArgument { Name = "s", Value = "A string" } },
                "s + \" (with greetings from a csx script)\""
            );
            return script;
        }

        public string Guid => "B44E2D27-3E0A-4305-ABCA-73615BD4CB67";
    }
}