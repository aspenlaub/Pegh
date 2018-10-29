using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IDisguiser {
        Task<string> Disguise(string s, IErrorsAndInfos errorsAndInfos);
    }
}
