namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IComponentProvider {
        IDisguiser Disguiser { get; }
        IFolderDeleter FolderDeleter { get; }
        IFolderUpdater FolderUpdater { get; }
        IPassphraseProvider PassphraseProvider { get; }
        IPeghEnvironment PeghEnvironment { get; }
        IPowershellExecuter PowershellExecuter { get; }
        IPrimeNumberGenerator PrimeNumberGenerator { get; }
        ISecretRepository SecretRepository { get; }
        IXmlDeserializer XmlDeserializer { get; }
        IXmlSerializer XmlSerializer { get; }
        IXmlSchemer XmlSchemer { get; }
    }
}
