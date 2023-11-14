using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class MethodNamesFromStackFramesExtractor : IMethodNamesFromStackFramesExtractor {
    public IList<string> ExtractMethodNamesFromStackFrames() {
        var methodNamesWithDuplicates = new StackTrace()
                                        .GetFrames()
                                        .Select(GetMethodNameFromFrame)
                                        .Where(m => !string.IsNullOrEmpty(m))
                                        .ToList();
        var methodNames = new List<string>();
        foreach (var methodName in methodNamesWithDuplicates) {
            if (methodNames.Count > 0 && methodNames[^1] == methodName) { continue; }

            methodNames.Add(methodName);
        }
        return methodNames;
    }

    private string GetMethodNameFromFrame(StackFrame stackFrame) {
        var method = stackFrame.GetMethod();
        if (method == null) { return null;  }

        var declaringType = method.DeclaringType?.GetTypeInfo();
        if (declaringType?.FullName == null) {
            return ExtractMethodName(method.Name);
        }

        var isAsyncStateMachine = typeof(IAsyncStateMachine).GetTypeInfo().IsAssignableFrom(declaringType);
        return ExtractMethodName(isAsyncStateMachine ? declaringType.FullName : method.Name);
    }

    private readonly List<string> _UnwantedMethods = new() {
        "BackgroundJobProcessor",
        "Dispatch",
        "Invoke", "InvokeMethod", "InvokeAsSynchronousTask", "InterpretedInvoke_Method", "InvokeWithNoArgs",
        "InnerInvoke", "InvokeExecutor",
        "Execute", "ExecuteEntryUnsafe", "ExecuteTestsInSource", "ExecuteWithThreadLocal", "ExecuteFromThreadPool",
        "ExecuteInternal", "ExecuteWithAbortSafety", "ExecuteTest", "ExecuteTests", "ExecuteTestsWithTestRunner",
        "OnMessageReceived",
        "RunTestMethod", "RunSingleTest", "RunTests", "RunFromThreadPoolDispatchLoop",
        "RunInternal", "RunTestInternalWithExecutors", "RunTestsInternal",
        "Start", "StartTestRun",
        "SafeProcessJob", "StartCallback",
        "WorkerThreadStart",
        ".ctor", ".cctor",
        nameof(ExtractMethodNamesFromStackFrames)
    };

    private string ExtractMethodName(string stringContainingMethodName) {
        stringContainingMethodName = stringContainingMethodName.Replace('+', '.');
        var ltPos = stringContainingMethodName.IndexOf('<');
        var gtPos = ltPos >= 0 ? stringContainingMethodName.IndexOf('>', ltPos) : -1;
        var methodName = ltPos >= 0 && gtPos > 0 ? stringContainingMethodName.Substring(ltPos + 1, gtPos - ltPos - 1) : stringContainingMethodName;
        methodName = _UnwantedMethods.Contains(methodName) ? null : methodName;
        return methodName;
    }
}