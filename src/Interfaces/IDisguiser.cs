using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IDisguiser {
        Task<string> Disguise(ISecretRepository secretRepository, string s, IErrorsAndInfos errorsAndInfos);
    }
}
