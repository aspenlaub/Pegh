using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

[assembly: InternalsVisibleTo("Aspenlaub.Net.GitHub.CSharp.Pegh.Test")]
namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class SecretRepository : ISecretRepository {
    internal readonly Dictionary<string, object> Values;
    internal SecretShouldDefaultSecretsBeStored SecretShouldDefaultSecretsBeStored;

    protected readonly ICsLambdaCompiler CsLambdaCompiler;
    protected readonly IDisguiser Disguiser;
    protected readonly IPeghEnvironment PeghEnvironment;
    protected readonly IXmlDeserializer XmlDeserializer;
    protected readonly IXmlSchemer XmlSchemer;
    protected readonly IXmlSerializer XmlSerializer;

    public SecretRepository(ICsLambdaCompiler csLambdaCompiler, IDisguiser disguiser, IPeghEnvironment peghEnvironment, IXmlDeserializer xmlDeserializer, IXmlSerializer xmlSerializer, IXmlSchemer xmlSchemer) {
        CsLambdaCompiler = csLambdaCompiler;
        Disguiser = disguiser;
        PeghEnvironment = peghEnvironment;
        XmlDeserializer = xmlDeserializer;
        XmlSchemer = xmlSchemer;
        XmlSerializer = xmlSerializer;
        Values = [];
        string folder = PeghEnvironment.RootWorkFolder + @"\SecretSamples\";
        if (!Directory.Exists(folder)) {
            Directory.CreateDirectory(folder);
        }
        folder = PeghEnvironment.RootWorkFolder + @"\SecretRepository\";
        if (!Directory.Exists(folder)) {
            Directory.CreateDirectory(folder);
        }
    }

    public async Task SetAsync<TResult>(ISecret<TResult> secret, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new() {
        TResult valueOrDefault = await ValueOrDefaultAsync(secret, errorsAndInfos);
        string xml = XmlSerializer.Serialize(valueOrDefault);
        await WriteToFileAsync(secret, xml, false, errorsAndInfos);
        Values[secret.Guid] = valueOrDefault;
    }

    internal async Task<TResult> ValueOrDefaultAsync<TResult>(ISecret<TResult> secret, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new() {
        if (Values.TryGetValue(secret.Guid, out object value)) {
            return value as TResult;
        }

        if (File.Exists(FileName(secret, false))) {
            string xml = await ReadFromFileAsync(secret, false, errorsAndInfos);
            Values[secret.Guid] = XmlDeserializer.Deserialize<TResult>(xml);
            return (TResult) Values[secret.Guid];
        }

        TResult clone = secret.DefaultValue.Clone();
        Values[secret.Guid] = clone;
        return clone;
    }

    public async Task<TResult> GetAsync<TResult>(ISecret<TResult> secret, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new() {
        TResult valueOrDefault;

        SaveSample(secret, false);

        string fileName = FileName(secret, false);
        if (!File.Exists(fileName)) {
            if (SecretShouldDefaultSecretsBeStored == null) {
                SecretShouldDefaultSecretsBeStored = new SecretShouldDefaultSecretsBeStored();
                await GetAsync(SecretShouldDefaultSecretsBeStored, errorsAndInfos);
            }
            ShouldDefaultSecretsBeStored shouldDefaultSecretsBeStored = await ValueOrDefaultAsync(SecretShouldDefaultSecretsBeStored, errorsAndInfos);
            if (!shouldDefaultSecretsBeStored.AutomaticallySaveDefaultSecretIfAbsent) {
                SaveSample(secret, true);
                string defaultFileName = FileName(secret, true);
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.PleaseLoadSecretSampleAdjustAndThenSaveAs, defaultFileName, fileName));
                return null;
            }

            await SetAsync(secret, errorsAndInfos);
            return await ValueOrDefaultAsync(secret, errorsAndInfos);
        }

        if (Values.ContainsKey(secret.Guid)) {
            valueOrDefault = await ValueOrDefaultAsync(secret, errorsAndInfos);
            return valueOrDefault;
        }

        string xml = await ReadFromFileAsync(secret, false, errorsAndInfos);
        if (string.IsNullOrEmpty(xml)) { return null; }

        valueOrDefault = XmlDeserializer.Deserialize<TResult>(xml);
        if (!IsGenericType(valueOrDefault.GetType())) {
            foreach (PropertyInfo property in valueOrDefault.GetType().GetProperties().Where(p => p.GetValue(valueOrDefault) == null)) {
                SaveSample(secret, true);
                string defaultFileName = FileName(secret, true);
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.AddedPropertyNotFoundInLoadedSecret, property.Name, fileName, defaultFileName));
            }
        }

        Values[secret.Guid] = valueOrDefault;
        return valueOrDefault;
    }

    protected static bool IsGenericType(Type t) {
        while (t != null) {
            if (t.IsGenericType) { return true; }

            t = t.BaseType;
        }

        return false;
    }

    public async Task<Func<TArgument, TResult>> CompileCsLambdaAsync<TArgument, TResult>(ICsLambda csLambda) {
        return await CsLambdaCompiler.CompileCsLambdaAsync<TArgument, TResult>(csLambda);
    }

    internal void Reset(IGuid secret) {
        Values.Remove(secret.Guid);

        string fileName = FileName(secret, false);
        if (!File.Exists(fileName)) { return; }

        File.Delete(fileName);
    }

    internal bool Exists(IGuid secret) {
        return File.Exists(FileName(secret, false));
    }

    internal void SaveSample<TResult>(ISecret<TResult> secret, bool update) where TResult : class, ISecretResult<TResult>, new() {
        string fileName = FileName(secret, true);
        if (fileName == null) {
            throw new NullReferenceException(nameof(fileName));
        }
        if (File.Exists(fileName) && !update) { return; }

        string xml = XmlSerializer.Serialize(secret.DefaultValue);
        if (File.Exists(fileName) && File.ReadAllText(fileName) == xml) { return; }

        File.WriteAllText(fileName, xml);
        File.WriteAllText(fileName.Replace(".xml", ".xsd"), XmlSchemer.Create(typeof(TResult)));
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    internal async Task WriteToFileAsync<TResult>(ISecret<TResult> secret, string xml, bool sample,
            IErrorsAndInfos errorsAndInfos)
                where TResult : class, ISecretResult<TResult>, new() {
        if (!XmlSchemer.Valid(secret.Guid, xml, typeof(TResult), errorsAndInfos)) { return; }

        string fileName = FileName(secret, sample);
        await File.WriteAllTextAsync(fileName, xml);
        await File.WriteAllTextAsync(fileName.Replace(".xml", ".xsd"), XmlSchemer.Create(typeof(TResult)));
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private async Task<string> ReadFromFileAsync<TResult>(ISecret<TResult> secret, bool sample,
            IErrorsAndInfos errorsAndInfos)
                where TResult : class, ISecretResult<TResult>, new() {
        string fileName = FileName(secret, sample);
        string xml = await File.ReadAllTextAsync(fileName);
        return XmlSchemer.Valid(secret.Guid, xml, typeof(TResult), errorsAndInfos) ? xml : "";
    }


    public string FileName(IGuid secret, bool sample) {
        return PeghEnvironment.RootWorkFolder
            + (sample ? @"\SecretSamples\" : @"\SecretRepository\") + secret.Guid + @".xml";
    }
}