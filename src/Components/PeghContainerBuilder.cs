using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components {
    public static class PeghContainerBuilder {
        public static ContainerBuilder RegisterForPegh(this ContainerBuilder builder, ICsArgumentPrompter csArgumentPrompter) {
            builder.RegisterInstance(csArgumentPrompter).As<ICsArgumentPrompter>();
            builder.RegisterType<CsLambdaCompiler>().As<ICsLambdaCompiler>();
            builder.RegisterType<Disguiser>().As<IDisguiser>();
            builder.RegisterType<FolderDeleter>().As<IFolderDeleter>();
            builder.RegisterType<FolderResolver>().As<IFolderResolver>();
            builder.RegisterType<FolderUpdater>().As<IFolderUpdater>();
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

        // ReSharper disable once UnusedMember.Global
        public static IServiceCollection UsePegh(this IServiceCollection services, ICsArgumentPrompter csArgumentPrompter) {
            services.AddSingleton(csArgumentPrompter);
            services.AddTransient<ICsLambdaCompiler, CsLambdaCompiler>();
            services.AddTransient<IDisguiser, Disguiser>();
            services.AddTransient<IFolderDeleter, FolderDeleter>();
            services.AddTransient<IFolderResolver, FolderResolver>();
            services.AddTransient<IFolderUpdater, FolderUpdater>();
            services.AddTransient<IPassphraseProvider, PassphraseProvider>();
            services.AddTransient<IPeghEnvironment, PeghEnvironment>();
            services.AddTransient<IPrimeNumberGenerator, PrimeNumberGenerator>();
            services.AddTransient<ISecretRepository, SecretRepository>();
            services.AddTransient<IStringCrypter, StringCrypter>();
            services.AddTransient<IXmlDeserializer, XmlDeserializer>();
            services.AddTransient<IXmlSerializer, XmlSerializer>();
            services.AddTransient<IXmlSchemer, XmlSchemer>();
            return services;
        }
    }
}
