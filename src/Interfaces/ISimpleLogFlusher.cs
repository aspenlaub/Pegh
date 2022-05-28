using System.Collections.Generic;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface ISimpleLogFlusher {
    HashSet<string> FileNames { get; }

    void Flush(ISimpleLogger logger, string subFolder);
}