﻿using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;

public class SecretShouldDefaultSecretsBeStored : ISecret<ShouldDefaultSecretsBeStored> {

    private ShouldDefaultSecretsBeStored _DefaultShouldDefaultSecretsBeStored;
    public ShouldDefaultSecretsBeStored DefaultValue => _DefaultShouldDefaultSecretsBeStored ??= new ShouldDefaultSecretsBeStored { AutomaticallySaveDefaultSecretIfAbsent = true };

    public string Guid => "49CAE10D-6BF2-4434-88C1-AD305AEF1838";
}