using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class SecretStringDecrypterFunction : ISecret<CsLambda> {
    private static CsLambda _defaultCsLambda;
    public CsLambda DefaultValue => _defaultCsLambda ??= CreateDefaultCsLambda();

    private static CsLambda CreateDefaultCsLambda() {
        var lambda = new CsLambda {
            LambdaExpression = "s => s.Substring(0, s.Length - 25)"
        };
        return lambda;
    }

    public string Guid => "BE7B608B-BE7BAB2C07AA-4241-B765-49B3";
}