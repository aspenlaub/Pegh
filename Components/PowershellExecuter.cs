using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class PowershellExecuter : IPowershellExecuter {
        public TResult ExecutePowershellFunction<TArgument, TResult>(IPowershellFunction<TArgument, TResult> powershellFunction, TArgument arg) where TResult : class {
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
            using (var powerShellInstance = PowerShell.Create()) {
                powerShellInstance.AddScript(script);
                powerShellInstance.AddParameter("secretArgument", arg);

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

                if (invokeResults.Count != 1) {
                    return null;
                }

                var invokeResult = invokeResults[0].BaseObject as PowershellFunctionResult;
                return invokeResult?.Result as TResult;
            }
        }

        public void ExecutePowershellScriptFile(string powershellScriptFileName, out IList<string> errors) {
            errors = new List<string>();
            var folder = powershellScriptFileName.Substring(0, powershellScriptFileName.LastIndexOf('\\'));

            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            var runSpaceInvoker = new RunspaceInvoke(runspace);
            runSpaceInvoker.Invoke("Set-ExecutionPolicy Unrestricted -Scope CurrentUser");
            var pipeline = runspace.CreatePipeline();
            var scriptCommand = new Command(powershellScriptFileName, true, false);
            pipeline.Commands.AddScript(folder[0] + ":");
            pipeline.Commands.AddScript("cd \"" + folder + "\"");
            pipeline.Commands.Add(scriptCommand);
            try {
                pipeline.Invoke();
            } catch (Exception e) {
                errors.Add(e.Message);
                return;
            }

            pipeline.Stop();
            runspace.Close();
            foreach(var error in pipeline.Error.ReadToEnd().Select(e => e.ToString())) {
                errors.Add(error);
            }
        }
    }
}