using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    public class FailingSecretStringFunction : ISecret<CsScript> {
        private static CsScript vDefaultCsScript;
        public CsScript DefaultValue => vDefaultCsScript ??
            (vDefaultCsScript = CreateDefaultCsScript());

        private static CsScript CreateDefaultCsScript() {
            var script = new CsScript(new List<CsScriptArgument> { new CsScriptArgument { Name = "s", Value = "A string"} },
                "va r r = s;", "r"
            );
            return script;
        }

        public string Guid => "26D50479-602B-402A-87FC-F9AB80C95791";
    }
}
