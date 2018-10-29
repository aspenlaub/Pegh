using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class SecretStringDecrypterFunction : ISecret<CsScript> {
        private static CsScript vDefaultCsScript;
        public CsScript DefaultValue => vDefaultCsScript ?? (vDefaultCsScript = CreateDefaultCsScript());

        private static CsScript CreateDefaultCsScript() {
            var script = new CsScript(new List<CsScriptArgument> { new CsScriptArgument { Name = "s", Value = "The encrypted string which the script shall decrypt" } },
                "s.Substring(0, s.Length - 25)"
            );
            return script;
        }

        public string Guid => "AB2C07AA-BE7B-4241-B765-4BE7B608B9B3";
    }
}
