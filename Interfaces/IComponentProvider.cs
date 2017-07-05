namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IComponentProvider {
        IAssemblyRepository AssemblyRepository { get; }
        IDisguiser Disguiser { get; }
        IFolderHelper FolderHelper { get; }
        IPassphraseProvider PassphraseProvider { get; }
        IPeghEnvironment PeghEnvironment { get; }
        IPrimeNumberGenerator PrimeNumberGenerator { get; }
        ISecretRepository SecretRepository { get; }
        IXmlDeserializer XmlDeserializer { get; }
        IXmlSerializer XmlSerializer { get; }
        IXmlSchemer XmlSchemer { get; }
    }
}
