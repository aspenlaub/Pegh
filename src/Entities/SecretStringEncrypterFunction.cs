using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class SecretStringEncrypterFunction : ISecret<CsScript> {
        private static CsScript vDefaultCsScript;
        public CsScript DefaultValue => vDefaultCsScript ?? (vDefaultCsScript = CreateDefaultCsScript());

        private static CsScript CreateDefaultCsScript() {
            var script = new CsScript(new List<CsScriptArgument> { new CsScriptArgument { Name = "s", Value = "The string which the script shall encrypt" } },
                "s + \" - but do not tell anyone\""
            );
            return script;
        }

        public string Guid => "8EA6005C-EF9C-4FF4-9CDC-179C3CA9D9E9";
    }
}