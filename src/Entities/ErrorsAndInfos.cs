using System.Collections.Generic;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class ErrorsAndInfos : IErrorsAndInfos {
    public IList<string> Errors { get; } = new List<string>();
    public IList<string> Infos { get; } = new List<string>();

    public bool AnyErrors() {
        return Errors.Any();
    }

    public bool AnyInfos() {
        return Infos.Any();
    }

    public override string ToString() {
        return this.ErrorsToString();
    }
}