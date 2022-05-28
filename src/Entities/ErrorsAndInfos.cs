using System.Collections.Generic;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class ErrorsAndInfos : IErrorsAndInfos {
    public IList<string> Errors { get; }
    public IList<string> Infos { get; }

    public ErrorsAndInfos() {
        Errors = new List<string>();
        Infos = new List<string>();
    }

    public bool AnyErrors() {
        return Errors.Any();
    }

    public bool AnyInfos() {
        return Infos.Any();
    }
}