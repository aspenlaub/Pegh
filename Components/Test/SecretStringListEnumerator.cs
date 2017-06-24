using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    public class SecretStringListEnumerator : IPowershellSecret<IList<string>, IEnumerable<string>> {
        private static PowershellFunction<IList<string>, IEnumerable<string>> vDefaultPowershellFunction;
        public IPowershellFunction<IList<string>, IEnumerable<string>> DefaultValue {
            get {
                return vDefaultPowershellFunction ??
                       (vDefaultPowershellFunction = new PowershellFunction<IList<string>, IEnumerable<string>> {
                           FunctionName = "Enumerate-List",
                           Script = "function Enumerate-List($list) {\r\n"
                                    + "\t$copyOfList = New-Object \"System.Collections.Generic.List[string]\"\r\n"
                                    + "\tForEach($listElement in $list) {\r\n"
                                    + "\t\t$copyOfList.Add($listElement)\r\n"
                                    + "\t}\r\n"
                                    + "\t$enumeratedList = [System.Collections.Generic.List[string]] $copyOfList\r\n"
                                    + "\t$result = New-Object -TypeName \"Aspenlaub.Net.GitHub.CSharp.Pegh.Entities.PowershellFunctionResult\"\r\n"
                                    + "\t$result.Result = $enumeratedList\r\n"
                                    + "\treturn $result\r\n"
                                    + "}"
                       });
            }
        }

        public string Guid {
            get { return "B18BE76D-74E1-4C50-95BF-1B15BB414FC9"; }
        }
    }
}
