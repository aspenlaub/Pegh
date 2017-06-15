using System;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces {
    public interface IAssemblyRepository {
        void AddToRepositoryIfNecessary(Type t);
        void AddToRepositoryIfNecessary(string sourceFileFullName, string ending);
        void UpdateIncludeFolder(string sourceFileFullName, out bool success, out int numberOfRepositoryFiles, out int numberOfCopiedFiles);
    }
}