using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IStringCrypter {
        Task<string> Encrypt(string s);
        Task<string> Decrypt(string s);
    }
}