using System.Collections.Generic;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface ISimpleLogReader {
    IList<ISimpleLogEntry> ReadLogFile(string fileName);
}