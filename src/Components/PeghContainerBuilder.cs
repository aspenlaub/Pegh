using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public static class PeghContainerBuilder {
    private static readonly ISimpleLogger SimpleLogger = new SimpleLogger(new SimpleLogFlusher());
    private static readonly ILogConfigurationFactory LogConfigurationFactory = new LogConfigurationFactory();

    public static ContainerBuilder UsePegh(this ContainerBuilder builder, ICsArgumentPrompter csArgumentPrompter) {
        builder.RegisterInstance(csArgumentPrompter).As<ICsArgumentPrompter>();
        builder.RegisterType<CsLambdaCompiler>().As<ICsLambdaCompiler>();
        builder.RegisterType<Disguiser>().As<IDisguiser>();
        builder.RegisterType<FolderDeleter>().As<IFolderDeleter>();
        builder.RegisterType<FolderResolver>().As<IFolderResolver>();
        builder.RegisterType<PassphraseProvider>().As<IPassphraseProvider>();
        builder.RegisterType<PeghEnvironment>().As<IPeghEnvironment>();
        builder.RegisterType<PrimeNumberGenerator>().As<IPrimeNumberGenerator>();
        builder.RegisterType<SecretRepository>().As<ISecretRepository>();
        builder.RegisterType<StringCrypter>().As<IStringCrypter>();
        builder.RegisterType<XmlDeserializer>().As<IXmlDeserializer>();
        builder.RegisterType<XmlSerializer>().As<IXmlSerializer>();
        builder.RegisterType<XmlSchemer>().As<IXmlSchemer>();
        builder.RegisterInstance(SimpleLogger);
        builder.RegisterInstance<ILogger>(SimpleLogger);
        builder.RegisterInstance(LogConfigurationFactory);

        return builder;
    }

    public static IServiceCollection UsePegh(this IServiceCollection services, ICsArgumentPrompter csArgumentPrompter) {
        services.AddSingleton(csArgumentPrompter);
        services.AddTransient<ICsLambdaCompiler, CsLambdaCompiler>();
        services.AddTransient<IDisguiser, Disguiser>();
        services.AddTransient<IFolderDeleter, FolderDeleter>();
        services.AddTransient<IFolderResolver, FolderResolver>();
        services.AddTransient<IPassphraseProvider, PassphraseProvider>();
        services.AddTransient<IPeghEnvironment, PeghEnvironment>();
        services.AddTransient<IPrimeNumberGenerator, PrimeNumberGenerator>();
        services.AddTransient<ISecretRepository, SecretRepository>();
        services.AddTransient<IStringCrypter, StringCrypter>();
        services.AddTransient<IXmlDeserializer, XmlDeserializer>();
        services.AddTransient<IXmlSerializer, XmlSerializer>();
        services.AddTransient<IXmlSchemer, XmlSchemer>();
        services.AddSingleton(SimpleLogger);
        services.AddSingleton<ILogger>(SimpleLogger);
        services.AddSingleton(LogConfigurationFactory);

        return services;
    }
}