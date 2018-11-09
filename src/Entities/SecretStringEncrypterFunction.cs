using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class SecretStringEncrypterFunction : ISecret<CsLambda> {
        private static CsLambda vDefaultCsScript;
        public CsLambda DefaultValue => vDefaultCsScript ?? (vDefaultCsScript = CreateDefaultCsLambda());

        private static CsLambda CreateDefaultCsLambda() {
            var lambda = new CsLambda {
                LambdaExpression = "s => s + \" - but do not tell anyone\""
            };
            return lambda;
        }

        public string Guid => "179C3CA5C-EF9C-4FF48EA600-9CDC-9D9E9";
    }
}