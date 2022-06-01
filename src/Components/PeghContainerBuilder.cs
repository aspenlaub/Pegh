﻿using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public static class PeghContainerBuilder {
    private static ISimpleLogger SimpleLogger;
    private static ILogConfiguration LogConfiguration;

    public static ContainerBuilder UsePegh(this ContainerBuilder builder, string applicationName, ICsArgumentPrompter csArgumentPrompter) {
        builder.RegisterInstance(csArgumentPrompter).As<ICsArgumentPrompter>();
        builder.RegisterType<CsLambdaCompiler>().As<ICsLambdaCompiler>();
        builder.RegisterType<Disguiser>().As<IDisguiser>();
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
        LogConfiguration ??= new LogConfiguration(applicationName);
        builder.RegisterInstance(LogConfiguration);
        SimpleLogger ??= new SimpleLogger(LogConfiguration, new SimpleLogFlusher(), new MethodNamesFromStackFramesExtractor());
        builder.RegisterInstance(SimpleLogger);
        builder.RegisterInstance<ILogger>(SimpleLogger);

        return builder;
    }

    public static IServiceCollection UsePegh(this IServiceCollection services, string applicationName, ICsArgumentPrompter csArgumentPrompter) {
        services.AddSingleton(csArgumentPrompter);
        services.AddTransient<ICsLambdaCompiler, CsLambdaCompiler>();
        services.AddTransient<IDisguiser, Disguiser>();
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
        LogConfiguration ??= new LogConfiguration(applicationName);
        services.AddSingleton(LogConfiguration);
        SimpleLogger ??= new SimpleLogger(LogConfiguration, new SimpleLogFlusher(), new MethodNamesFromStackFramesExtractor());
        services.AddSingleton(SimpleLogger);

        return services;
    }
}