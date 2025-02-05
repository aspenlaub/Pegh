﻿using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class Folder : IFolder {
    public string FullName { get; }

    public Folder(string fullName) {
        if (fullName != "" && fullName[^1] == '\\') {
            fullName = fullName[..^1];
        }

        FullName = fullName;
    }

    public override string ToString() {
        return FullName;
    }
}