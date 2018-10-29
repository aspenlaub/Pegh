using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IStringCrypter {
        Task<string> Encrypt(string s);
        Task<string> Decrypt(string s);
    }
}