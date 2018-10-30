using System.Globalization;
using System.Text.RegularExpressions;
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

            s = Regex.Replace(
                s,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });

            return s;
        }
    }
}
