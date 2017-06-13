using System.Collections.Generic;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface ISecret<in TArgument, TResult> : IGuid {

        SecretTypes SecretType { get; }

        IDictionary<string, string> Default();

        void Set(TResult secret);
        TResult Get();
        TResult Get(TArgument arg);
    }
}
