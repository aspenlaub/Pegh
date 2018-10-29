using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ICsScriptExecuter {
        Task<string> ExecuteCsScriptAsync(ICsScript csScript, IList<ICsScriptArgument> presetArguments, ICsScriptArgumentPrompter prompter);
    }
}
