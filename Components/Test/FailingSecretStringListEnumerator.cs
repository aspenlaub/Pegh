using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components.Test {
    public class FailingSecretStringListEnumerator : IPowershellSecret<IList<string>, IEnumerable<string>> {
        private static PowershellFunction<IList<string>, IEnumerable<string>> vDefaultPowershellFunction;
        public IPowershellFunction<IList<string>, IEnumerable<string>> DefaultValue {
            get {
                return vDefaultPowershellFunction ??
                       (vDefaultPowershellFunction = new PowershellFunction<IList<string>, IEnumerable<string>> {
                           FunctionName = "Enumerate-List",
                           Script = "function Enumerate-List($list) {\r\n"
                                    + "\t$result = New-Object -TypeName \"Aspenlaub.Net.GitHub.CSharp.Pegh.Entities.PowershellFunctionResult\"\r\n"
                                    + "\t$result.Result = $list\r\n"
                                    + "\tre turn $result\r\n"
                                    + "}"
                       });
            }
        }

        public string Guid {
            get { return "771D8299-01E5-4537-B039-C236E19FEB50"; }
        }
    }
}
