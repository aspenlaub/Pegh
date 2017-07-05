using System;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IXmlSchemer {
        string Create(Type t);
        bool Valid(string xml, Type t);
    }
}
