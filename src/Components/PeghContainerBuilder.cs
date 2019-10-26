using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public static class PeghContainerBuilder {
        public static ContainerBuilder UsePegh(this ContainerBuilder builder, ICsArgumentPrompter csArgumentPrompter) {
            builder.RegisterInstance(csArgumentPrompter).As<ICsArgumentPrompter>();
            builder.RegisterType<CsLambdaCompiler>().As<ICsLambdaCompiler>();
            builder.RegisterType<Disguiser>().As<IDisguiser>();
            builder.RegisterType<FolderDeleter>().As<IFolderDeleter>();
            builder.RegisterType<FolderResolver>().As<IFolderResolver>();
            builder.RegisterType<FolderUpdater>().As<IFolderUpdater>();
            builder.RegisterType<JsonDepsDifferencer>().As<IJsonDepsDifferencer>();
            builder.RegisterType<PassphraseProvider>().As<IPassphraseProvider>();
            builder.RegisterType<PeghEnvironment>().As<IPeghEnvironment>();
            builder.RegisterType<PrimeNumberGenerator>().As<IPrimeNumberGenerator>();
            builder.RegisterType<SecretRepository>().As<ISecretRepository>();
            builder.RegisterType<StringCrypter>().As<IStringCrypter>();
            builder.RegisterType<XmlDeserializer>().As<IXmlDeserializer>();
            builder.RegisterType<XmlSerializer>().As<IXmlSerializer>();
            builder.RegisterType<XmlSchemer>().As<IXmlSchemer>();
            return builder;
        }
    }
}
