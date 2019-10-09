using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    public static class PeghTestContainerBuilder {
        public static ContainerBuilder RegisterForPeghTest(this ContainerBuilder builder) {
            RegisterDefaultTypes(builder);
            builder.RegisterType<SecretRepository>().As<ISecretRepository>();
            builder.RegisterType<PeghEnvironment>().As<IPeghEnvironment>();
            builder.RegisterType<Disguiser>().As<IDisguiser>();
            var csArgumentPrompterMock = new Mock<ICsArgumentPrompter>();
            builder.RegisterInstance(csArgumentPrompterMock.Object).As<ICsArgumentPrompter>();
            return builder;
        }

        public static ContainerBuilder RegisterForPeghTest(this ContainerBuilder builder, ICsArgumentPrompter csArgumentPrompter) {
            RegisterDefaultTypes(builder);
            builder.RegisterType<SecretRepository>().As<ISecretRepository>();
            builder.RegisterType<PeghEnvironment>().As<IPeghEnvironment>();
            builder.RegisterType<Disguiser>().As<IDisguiser>();
            builder.RegisterInstance(csArgumentPrompter).As<ICsArgumentPrompter>();
            return builder;
        }

        public static ContainerBuilder RegisterForPeghTest(this ContainerBuilder builder, ISecretRepository secretRepository) {
            RegisterDefaultTypes(builder);
            builder.RegisterInstance(secretRepository).As<ISecretRepository>();
            builder.RegisterType<PeghEnvironment>().As<IPeghEnvironment>();
            builder.RegisterType<Disguiser>().As<IDisguiser>();
            return builder;
        }

        public static ContainerBuilder RegisterForPeghTest(this ContainerBuilder builder, IPeghEnvironment peghEnvironment, ICsArgumentPrompter csArgumentPrompter) {
            RegisterDefaultTypes(builder);
            builder.RegisterType<SecretRepository>().As<ISecretRepository>();
            builder.RegisterInstance(peghEnvironment).As<IPeghEnvironment>();
            builder.RegisterType<Disguiser>().As<IDisguiser>();
            builder.RegisterInstance(csArgumentPrompter).As<ICsArgumentPrompter>();
            return builder;
        }

        public static ContainerBuilder RegisterForPeghTest(this ContainerBuilder builder, IDisguiser disguiser, ICsArgumentPrompter csArgumentPrompter) {
            RegisterDefaultTypes(builder);
            builder.RegisterType<SecretRepository>().As<ISecretRepository>();
            builder.RegisterType<PeghEnvironment>().As<IPeghEnvironment>();
            builder.RegisterInstance(disguiser).As<IDisguiser>();
            builder.RegisterInstance(csArgumentPrompter).As<ICsArgumentPrompter>();
            return builder;
        }

        private static void RegisterDefaultTypes(ContainerBuilder builder) {
            builder.RegisterType<CsLambdaCompiler>().As<ICsLambdaCompiler>();
            builder.RegisterType<FolderDeleter>().As<IFolderDeleter>();
            builder.RegisterType<FolderResolver>().As<IFolderResolver>();
            builder.RegisterType<FolderUpdater>().As<IFolderUpdater>();
            builder.RegisterType<PassphraseProvider>().As<IPassphraseProvider>();
            builder.RegisterType<PrimeNumberGenerator>().As<IPrimeNumberGenerator>();
            builder.RegisterType<StringCrypter>().As<IStringCrypter>();
            builder.RegisterType<XmlDeserializer>().As<IXmlDeserializer>();
            builder.RegisterType<XmlSerializer>().As<IXmlSerializer>();
            builder.RegisterType<XmlSchemer>().As<IXmlSchemer>();
        }
    }
}
