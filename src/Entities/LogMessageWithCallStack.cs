using System.Collections.Generic;
using System.Text.Json;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

internal class LogMessageWithCallStack {
    public string Message { get; set; }
    public IList<string> MethodNamesInCallStack { get; set; }

    public static string CreateJson(string message, IList<string> methodNamesInCallStack) {
        return JsonSerializer.Serialize(new LogMessageWithCallStack { Message = message, MethodNamesInCallStack = methodNamesInCallStack });
    }
}