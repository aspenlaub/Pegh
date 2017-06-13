namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IXmlSerializer {
        string Serialize<TItemType>(TItemType item);
    }
}
