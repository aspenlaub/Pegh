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
        foreach (string methodName in methodNamesWithDuplicates) {
            if (methodNames.Count > 0 && methodNames[^1] == methodName) { continue; }

            methodNames.Add(methodName);
        }
        return methodNames;
    }

    private string GetMethodNameFromFrame(StackFrame stackFrame) {
        MethodBase method = stackFrame.GetMethod();
        if (method == null) { return null;  }

        TypeInfo declaringType = method.DeclaringType?.GetTypeInfo();
        if (declaringType?.FullName == null) {
            return ExtractMethodName(method.Name);
        }

        bool isAsyncStateMachine = typeof(IAsyncStateMachine).GetTypeInfo().IsAssignableFrom(declaringType);
        return ExtractMethodName(isAsyncStateMachine ? declaringType.FullName : method.Name);
    }

    private readonly List<string> _UnwantedMethods = [
        "BackgroundJobProcessor",

        "Dispatch",

        "Execute", "ExecuteEntryUnsafe", "ExecuteTestsInSource",
        "ExecuteWithThreadLocal", "ExecuteFromThreadPool",
        "ExecuteInternal",
        "ExecuteWithAbortSafety", "ExecuteTest", "ExecuteTests",
        "ExecuteTestsWithTestRunner", "ExecutionContextCallback",

        "FinishSlow",

        "Get", "GetInvokeResult",

        "Invoke",
        "InvokeMethod", "InvokeAsSynchronousTask", "InterpretedInvoke_Method",
        "InvokeWithNoArgs",
        "InnerInvoke", "InvokeExecutor",
        "InternalReadAllText",

        "MoveNext",

        "OnMessageReceived",

        "ProcessInnerTask",

        "Resolve", "ReadAsyncInternal", "ReadBuffer", "ReadAsyncSlowPath",
        "RunTestMethod",
        "RunSingleTest", "RunTests", "RunFromThreadPoolDispatchLoop",
        "RunOnContext",
        "RunInternal", "RunTestInternalWithExecutors",
        "RunTestsFromRightContext",
        "RunTestsInternal",
        "RunOrScheduleAction", "RunContinuations", "ReadFromFile",

        "Start", "StartTestRun",
        "SafeProcessJob", "StartCallback",
        "SetExistingTaskResult", "SetResult",

        "TrySetResult", "TrySetFromTask",

        "WorkerThreadStart",

        ".ctor", ".cctor",

        nameof(ExtractMethodNamesFromStackFrames)
    ];

    private string ExtractMethodName(string stringContainingMethodName) {
        stringContainingMethodName = stringContainingMethodName.Replace('+', '.');
        int ltPos = stringContainingMethodName.IndexOf('<');
        int gtPos = ltPos >= 0 ? stringContainingMethodName.IndexOf('>', ltPos) : -1;
        string methodName = ltPos >= 0 && gtPos > 0 ? stringContainingMethodName.Substring(ltPos + 1, gtPos - ltPos - 1) : stringContainingMethodName;
        string syncMethodName = methodName.EndsWith("Async") ? methodName.Substring(0, methodName.Length - 5) : methodName;
        methodName = _UnwantedMethods.Contains(methodName) || _UnwantedMethods.Contains(syncMethodName) ? null : methodName;
        return methodName;
    }
}