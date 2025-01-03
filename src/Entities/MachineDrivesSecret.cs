﻿using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class MachineDrivesSecret : ISecret<MachineDrives> {
    private MachineDrives _DefaultMachineDrives;
    public MachineDrives DefaultValue => _DefaultMachineDrives ??= [new() { Machine = Environment.MachineName, Name = "MachineTest", Drive = "c" }];

    public string Guid => "E554D5A8-21C6-4C33-A65D-D14E706AEEAB";
}