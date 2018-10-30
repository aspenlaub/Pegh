using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class CsScriptMarshaller : ICsScriptMarshaller {
        public string ToCsScript(string s) {
            return s.Replace("\"", "\\\"");
        }

        public string FromCsScript(string s) {
            if (s.StartsWith("\"") && s.EndsWith("\"")) {
                s = s.Substring(1, s.Length - 2).Replace("\\\"", "\"").Replace("\\\\", "\\");
            }

            return s;
        }
    }
}
