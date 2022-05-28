using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Components;

public class PeghEnvironment : IPeghEnvironment {
    public string RootWorkFolder { get; }

    // ReSharper disable once UnusedMember.Global
    public PeghEnvironment() {
        RootWorkFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Aspenlaub.Net";
    }

    public PeghEnvironment(IFolder folder) {
        RootWorkFolder = folder.FullName;
    }
}