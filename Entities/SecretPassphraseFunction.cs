using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class SecretPassphraseFunction : ISecret<PowershellFunction<SecretPassphraseFunctionArgument, string>> {
        private static PowershellFunction<SecretPassphraseFunctionArgument, string> vDefaultPowershellFunction;
        public PowershellFunction<SecretPassphraseFunctionArgument, string> DefaultValue {
            get {
                return vDefaultPowershellFunction ??
                       (vDefaultPowershellFunction = new PowershellFunction<SecretPassphraseFunctionArgument, string> {
                           FunctionName = "Get-Passphrase",
                           Script = "function Get-Passphrase($arg) {\r\n"
                                    + "\t$result = New-Object -TypeName \"Aspenlaub.Net.GitHub.CSharp.Pegh.Entities.PowershellFunctionResult\"\r\n"
                                    + "\tif ($arg.IsUserPresent) {\r\n"
                                    + "\t\tAdd-Type -Path \"Aspenlaub.Net.GitHub.CSharp.Pegh.GUI.dll\"\r\n"
                                    + "\t\t$dialog = New-Object -TypeName \"Aspenlaub.Net.GitHub.CSharp.Pegh.GUI.EnterPassphrase\"\r\n"
                                    + "\t\t$provider = New-Object -TypeName \"Aspenlaub.Net.GitHub.CSharp.Pegh.Components.PassphraseProvider\"\r\n"
                                    + "\t\t$result.Result = $provider.Passphrase($dialog)\r\n"
                                    + "\t} else {\r\n"
                                    + "\t\t$result.Result = $arg.PassphraseIfUserIsNotPresent\r\n"
                                    + "\t}\r\n"
                                    + "\treturn $result\r\n"
                                    + "}"
                       });
            }
        }

        public string Guid {
            get { return "7CBBB992-78EB-413F-9460-2C83450EF3E2"; }
        }
    }
}
