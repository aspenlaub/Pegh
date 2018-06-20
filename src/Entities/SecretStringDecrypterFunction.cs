using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class SecretStringDecrypterFunction : ISecret<PowershellFunction<string, string>> {
        private static PowershellFunction<string, string> vDefaultPowershellFunction;
        public PowershellFunction<string, string> DefaultValue {
            get {
                return vDefaultPowershellFunction ??
                       (vDefaultPowershellFunction = new PowershellFunction<string, string> {
                           FunctionName = "Decrypt-String",
                           Script = "\r\n\tfunction Decrypt-String($s) {\r\n"
                                    + "\t\t$result = New-Object -TypeName \"Aspenlaub.Net.GitHub.CSharp.Pegh.Entities.PowershellFunctionResult\"\r\n"
                                    + "\t\t$result.Result = $s.Substring(0, $s.Length - 25)\r\n"
                                    + "\t\treturn $result\r\n"
                                    + "\t}\r\n"
                       });
            }
        }

        public string Guid { get { return "F2239CAF-A303-45A1-A0CA-B20CEAFB8CC1"; } }
    }
}
