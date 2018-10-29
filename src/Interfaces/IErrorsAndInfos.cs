using System.Collections.Generic;
// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IErrorsAndInfos {
        IList<string> Errors { get; }
        IList<string> Infos { get; }

        bool AnyErrors();
        bool AnyInfos();
    }
}
