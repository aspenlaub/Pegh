using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public static class PeghContainerBuilder {
    private static ISimpleLogger _simpleLogger;
    private static ILogConfiguration _logConfiguration;

    public static ContainerBuilder UsePegh(this ContainerBuilder builder, string applicationName, ICsArgumentPrompter csArgumentPrompter) {
        builder.RegisterInstance(csArgumentPrompter).As<ICsArgumentPrompter>();
        builder.RegisterType<CsLambdaCompiler>().As<ICsLambdaCompiler>();
        builder.RegisterType<Disguiser>().As<IDisguiser>();
        builder.RegisterType<ExceptionFolderProvider>().As<IExceptionFolderProvider>();
        builder.RegisterType<FolderDeleter>().As<IFolderDeleter>();
        builder.RegisterType<FolderResolver>().As<IFolderResolver>();
        builder.RegisterType<MethodNamesFromStackFramesExtractor>().As<IMethodNamesFromStackFramesExtractor>();
        builder.RegisterType<PassphraseProvider>().As<IPassphraseProvider>();
        builder.RegisterType<PeghEnvironment>().As<IPeghEnvironment>();
        builder.RegisterType<PrimeNumberGenerator>().As<IPrimeNumberGenerator>();
        builder.RegisterType<SecretRepository>().As<ISecretRepository>();
        builder.RegisterType<SimpleLogReader>().As<ISimpleLogReader>();
        builder.RegisterType<StringCrypter>().As<IStringCrypter>();
        builder.RegisterType<XmlDeserializer>().As<IXmlDeserializer>();
        builder.RegisterType<XmlSerializer>().As<IXmlSerializer>();
        builder.RegisterType<XmlSchemer>().As<IXmlSchemer>();
        _logConfiguration ??= new LogConfiguration(applicationName);
        builder.RegisterInstance(_logConfiguration);
        var exceptionFolderProvider = new ExceptionFolderProvider();
        _simpleLogger ??= new SimpleLogger(_logConfiguration, new SimpleLogFlusher(exceptionFolderProvider), new MethodNamesFromStackFramesExtractor(), exceptionFolderProvider);
        builder.RegisterInstance(_simpleLogger);
        builder.RegisterInstance<ILogger>(_simpleLogger);

        return builder;
    }

    public static IServiceCollection UsePegh(this IServiceCollection services, string applicationName, ICsArgumentPrompter csArgumentPrompter) {
        services.AddSingleton(csArgumentPrompter);
        services.AddTransient<ICsLambdaCompiler, CsLambdaCompiler>();
        services.AddTransient<IDisguiser, Disguiser>();
        services.AddTransient<IExceptionFolderProvider, ExceptionFolderProvider>();
        services.AddTransient<IFolderDeleter, FolderDeleter>();
        services.AddTransient<IFolderResolver, FolderResolver>();
        services.AddTransient<IMethodNamesFromStackFramesExtractor, MethodNamesFromStackFramesExtractor>();
        services.AddTransient<IPassphraseProvider, PassphraseProvider>();
        services.AddTransient<IPeghEnvironment, PeghEnvironment>();
        services.AddTransient<IPrimeNumberGenerator, PrimeNumberGenerator>();
        services.AddTransient<ISecretRepository, SecretRepository>();
        services.AddTransient<ISimpleLogReader, SimpleLogReader>();
        services.AddTransient<IStringCrypter, StringCrypter>();
        services.AddTransient<IXmlDeserializer, XmlDeserializer>();
        services.AddTransient<IXmlSerializer, XmlSerializer>();
        services.AddTransient<IXmlSchemer, XmlSchemer>();
        _logConfiguration ??= new LogConfiguration(applicationName);
        services.AddSingleton(_logConfiguration);
        var exceptionFolderProvider = new ExceptionFolderProvider();
        _simpleLogger ??= new SimpleLogger(_logConfiguration, new SimpleLogFlusher(exceptionFolderProvider), new MethodNamesFromStackFramesExtractor(), exceptionFolderProvider);
        services.AddSingleton(_simpleLogger);

        return services;
    }
}