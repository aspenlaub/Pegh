﻿namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IComponentProvider {
        void SetAppDataSpecialFolder(IFolder folder);

        ICsScriptExecuter CsScriptExecuter { get; }
        IDisguiser Disguiser { get; }
        IFolderDeleter FolderDeleter { get; }
        IFolderUpdater FolderUpdater { get; }
        IPassphraseProvider PassphraseProvider { get; }
        IPeghEnvironment PeghEnvironment { get; }
        IPrimeNumberGenerator PrimeNumberGenerator { get; }
        ISecretRepository SecretRepository { get; }
        IStringCrypter StringCrypter { get; }
        IXmlDeserializer XmlDeserializer { get; }
        IXmlSerializer XmlSerializer { get; }
        IXmlSchemer XmlSchemer { get; }
    }
}
