namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IXmlDeserializer {
        TItemType Deserialize<TItemType>(string xml);
    }
}
