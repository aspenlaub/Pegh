// ReSharper disable UnusedMember.Global
using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface IStringCrypter {
    Task<string> EncryptAsync(string s);
    Task<string> DecryptAsync(string s);
}