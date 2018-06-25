namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IStringCrypter {
        string Encrypt(string s);
        string Decrypt(string s);
    }
}