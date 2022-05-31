using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;

public static class LoggerExtensions {
    public static void LogInformationWithCallStack(this ILogger logger, string message, IList<string> methodNamesInCallStack) {
        logger.LogInformation(LogMessageWithCallStack.CreateJson(message, methodNamesInCallStack));
    }

    public static void LogWarningWithCallStack(this ILogger logger, string message, IList<string> methodNamesInCallStack) {
        logger.LogWarning(LogMessageWithCallStack.CreateJson(message, methodNamesInCallStack));
    }

    public static void LogErrorWithCallStack(this ILogger logger, string message, IList<string> methodNamesInCallStack) {
        logger.LogError(LogMessageWithCallStack.CreateJson(message, methodNamesInCallStack));
    }
}