using System;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IXmlSchemer {
        string Create(Type t);
        bool Valid(string secretGuid, string xml, Type t, IErrorsAndInfos errorsAndInfos);
    }
}
