using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class PowershellExecuter : IPowershellExecuter {
        public TResult ExecutePowershellFunction<TArgument, TResult>(IPowershellFunction<TArgument, TResult> powershellFunction, TArgument arg) where TResult : class {
            bool success;

            var script = "Param(\r\n"
                         + "\t[Parameter(Mandatory=$true)]\r\n"
                         + "\t$secretArgument\r\n"
                         + ")\r\n\r\n"
                         + "Add-Type -Path \""
                         + typeof(PowershellFunctionResult).Assembly.Location
                         + "\"\r\n\r\n"
                         + powershellFunction.Script
                         + "\r\n\r\n"
                         + powershellFunction.FunctionName + "($secretArgument)\r\n";
            return ExecutePowershellFunction<TResult>(script, powerShellInstance => powerShellInstance.AddParameter("secretArgument", arg), out success);
        }

        public bool ExecutePowershellFunction(string powershellScriptContents) {
            bool success;

            ExecutePowershellFunction<object>(powershellScriptContents, powerShellInstance => { }, out success);
            return success;
        }

        protected TResult ExecutePowershellFunction<TResult>(string powershellScriptContents, Action<PowerShell> setArguments, out bool success) where TResult : class {
            using (var powerShellInstance = PowerShell.Create()) {
                success = false;
                powerShellInstance.AddScript(powershellScriptContents);
                setArguments(powerShellInstance);

                var runSpace = RunspaceFactory.CreateRunspace();
                runSpace.ApartmentState = ApartmentState.STA;
                runSpace.ThreadOptions = PSThreadOptions.ReuseThread;
                runSpace.Open();
                powerShellInstance.Runspace = runSpace;
                Collection<PSObject> invokeResults;
                try {
                    invokeResults = powerShellInstance.Invoke();
                    if (powerShellInstance.Streams.Error.Count > 0) {
                        return null;
                    }
                } catch {
                    return null;
                }

                success = true;
                if (invokeResults.Count != 1) {
                    return null;
                }

                var invokeResult = invokeResults[0].BaseObject as PowershellFunctionResult;
                return invokeResult?.Result as TResult;
            }
        }
    }
}
