using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface IDisguiser {
    Task<string> Disguise(ISecretRepository secretRepository, string s, IErrorsAndInfos errorsAndInfos);
}