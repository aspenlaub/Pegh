using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class CsScript : ICsScript, ISecretResult<CsScript> {
        public List<CsScriptArgument> StringArgumentNameToDescriptions { get; set; }
        public string Script { get; set; }
        public int TimeoutInSeconds { get; set; }

        public CsScript() : this(new List<CsScriptArgument>()) {
        }

        public CsScript(List<CsScriptArgument> stringArgumentNameToDescriptions, params string[] scriptLines) {
            StringArgumentNameToDescriptions = stringArgumentNameToDescriptions;
            Script = string.Join("\r\n    ", scriptLines) + "\r\n    #exit\r\n  ";
            TimeoutInSeconds = 5;
        }

        public CsScript Clone() {
            return new CsScript {
                StringArgumentNameToDescriptions = StringArgumentNameToDescriptions,
                Script = Script,
                TimeoutInSeconds = TimeoutInSeconds
            };
        }
    }
}
