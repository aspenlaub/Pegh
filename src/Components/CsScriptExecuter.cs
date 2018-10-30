using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Dotnet.Script.Core;
using Dotnet.Script.DependencyModel.Logging;
using Dotnet.Script.DependencyModel.Runtime;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public class CsScriptExecuter : ICsScriptExecuter {
        public async Task<string> ExecuteCsScriptAsync(ICsScript csScript, IList<ICsScriptArgument> presetArguments, ICsScriptArgumentPrompter prompter) {
            if (csScript.Script.Length == 0 || !csScript.Script.TrimEnd().EndsWith("#exit")) {
                throw new Exception("Csx scripts must end with #exit");
            }

            var scriptLines = csScript.Script.Replace("\r", "").Split('\n').Select(s => s.TrimStart()).ToList();
            foreach (var argument in csScript.StringArgumentNameToDescriptions) {
                var presetArgument = presetArguments.FirstOrDefault(p => p.Name == argument.Name);
                string argumentValue;
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (presetArgument == null) {
                    if (prompter == null) {
                        throw new NullReferenceException($"You are attempting to use a secret that requires user interaction. Please provide a {nameof(ICsScriptArgumentPrompter)}");
                    }

                    argumentValue = prompter.PromptForArgument(argument.Name, argument.Value);
                } else {
                    argumentValue = presetArgument.Value;
                }

                scriptLines.Insert(0, "var " + argument.Name + " = @\"" + argumentValue.Replace("\"", "\\\"") + "\";");
            }
            var context = GetRunner(scriptLines.ToArray());
            var delayTask = Task.Delay(TimeSpan.FromSeconds(csScript.TimeoutInSeconds));
            if (delayTask == await Task.WhenAny(Task.Factory.StartNew(() => context.Runner.RunLoop()), delayTask)) {
                throw new OperationCanceledException($"Csx script execution did not finish within the allowed timespan of {csScript.TimeoutInSeconds} seconds");
            }

            if (context.Console.Error.ToString() != "") {
                throw new Exception($"An error occurred during csx script execution: {context.Console.Error}");
            }

            var output = ProcessOutput(context.Console.Out.ToString());
            return output;
        }

        private class InteractiveTestContext {
            public InteractiveTestContext(ScriptConsole console, InteractiveRunner runner) {
                Console = console;
                Runner = runner;
            }

            public ScriptConsole Console { get; }
            public InteractiveRunner Runner { get; }
        }

        private static LogFactory CreateLogFactory() {
            return type => (level, message, exception) => {
                if (level == LogLevel.Info || level == LogLevel.Debug || level == LogLevel.Trace) { return; }

                throw new NotImplementedException();
            };
        }

        private InteractiveTestContext GetRunner(string[] commands) {
            var reader = new StringReader(string.Join(Environment.NewLine, commands));
            var writer = new StringWriter();
            var error = new StringWriter();

            var console = new ScriptConsole(writer, reader, error);

            var logFactory = CreateLogFactory();
            var runtimeDependencyResolver = new RuntimeDependencyResolver(logFactory);

            var compiler = new ScriptCompiler(logFactory, runtimeDependencyResolver);
            var runner = new InteractiveRunner(compiler, logFactory, console, Array.Empty<string>());
            return new InteractiveTestContext(console, runner);
        }

        private string ProcessOutput(string s) {
            while (s.StartsWith("> ") || s.StartsWith("* ")) {
                s = s.Substring(2);
            }
            while (s.EndsWith("> ") || s.EndsWith("\r\n")) {
                s = s.Substring(0, s.Length - 2);
            }

            if (s.StartsWith("\"") && s.EndsWith("\"")) {
                s = s.Substring(1, s.Length - 2).Replace("\\\"", "\"").Replace("\\\\", "\\");
            }

            return s;
        }
    }
}
