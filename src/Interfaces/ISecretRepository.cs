using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ISecretRepository {
        ICsScriptArgumentPrompter CsScriptArgumentPrompter { get; set; }

        Task SetAsync<TResult>(ISecret<TResult> secret, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new();
        Task<TResult> GetAsync<TResult>(ISecret<TResult> secret, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new();
        Task<string> ExecuteCsScriptAsync(ICsScript csScript, IList<ICsScriptArgument> presetArguments);
    }
}
