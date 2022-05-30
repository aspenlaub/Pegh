using System.Collections.Generic;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IMethodNamesFromStackFramesExtractor {
        IList<string> ExtractMethodNamesFromStackFrames();
    }
}
