using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

public class SecretStringFunction : ISecret<CsLambda> {
    private static CsLambda DefaultCsLambda;
    public CsLambda DefaultValue => DefaultCsLambda ??= CreateDefaultCsLambda();

    private static CsLambda CreateDefaultCsLambda() {
        var lambda = new CsLambda {
            LambdaExpression = "s => s + \" (with greetings from a csx script)\""
        };
        return lambda;
    }

    public string Guid => "5BD4B2D27-3E0A-44E4305-ABCA-7361CB67";
}