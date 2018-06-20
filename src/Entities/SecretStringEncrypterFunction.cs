using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class SecretStringEncrypterFunction : ISecret<PowershellFunction<string, string>> {
        private static PowershellFunction<string, string> vDefaultPowershellFunction;
        public PowershellFunction<string, string> DefaultValue {
            get {
                return vDefaultPowershellFunction ??
                       (vDefaultPowershellFunction = new PowershellFunction<string, string> {
                           FunctionName = "Encrypt-String",
                           Script = "\r\n\tfunction Encrypt-String($s) {\r\n"
                                    + "\t\t$result = New-Object -TypeName \"Aspenlaub.Net.GitHub.CSharp.Pegh.Entities.PowershellFunctionResult\"\r\n"
                                    + "\t\t$result.Result = $s + \" - but do not tell anyone\"\r\n"
                                    + "\t\treturn $result\r\n"
                                    + "\t}\r\n"
                       });
            }
        }

        public string Guid { get { return "6F829821-0C10-47C1-A298-A8AECC456D37"; } }
    }
}
