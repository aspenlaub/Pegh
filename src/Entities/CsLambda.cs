﻿using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class CsLambda : ICsLambda, ISecretResult<CsLambda> {
        public List<string> Namespaces { get; set; } = new List<string>();
        public List<string> Types { get; set; } = new List<string>();
        public string LambdaExpression { get; set; }

        public CsLambda Clone() {
            var clone = new CsLambda();
            clone.Namespaces.AddRange(Namespaces);
            clone.Types.AddRange(Types);
            clone.LambdaExpression = LambdaExpression;
            return clone;
        }
    }
}