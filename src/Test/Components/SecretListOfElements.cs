﻿using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components;

public class SecretListOfElements : ISecret<ListOfElements> {
    private ListOfElements _DefaultListOfElements;
    public ListOfElements DefaultValue => _DefaultListOfElements ??= [];

    public string Guid => "DC6A816E-9C63-510B-4B24-3265C9B03FCF";
}