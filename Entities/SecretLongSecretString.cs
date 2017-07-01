using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Entities {
    public class SecretLongSecretString : ISecret<LongSecretString> {
        private LongSecretString vDefaultValue;

        public LongSecretString DefaultValue {
            get { return vDefaultValue ?? (vDefaultValue = new LongSecretString { LongString = GenerateLongString(128) }); }
        }

        public string Guid { get { return "B2C6C45C-C77F-4227-9BC1-62419AC4BB3C"; } }

        private static string GenerateLongString(int length) {
            const string pool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!$%/#*.";
            var s = "";
            var random = new Random();
            for (var i = 0; i < length; i++) {
                s = s + pool[random.Next(pool.Length)];
            }

            return s;
        }
    }
}
