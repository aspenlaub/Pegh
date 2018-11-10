﻿using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Test.Components {
    [TestClass]
    public class StringCrypterTest {
        [TestMethod]
        public void CanEncryptStringWithoutEscapingUnicodeCharacters() {
            var componentProvider = new ComponentProvider();
            var sut = componentProvider.StringCrypter;
            var encrypted = sut.Encrypt("2018-10-3076MuWwlgbBtCHxwW");
            Assert.IsFalse(encrypted.Contains("\\u"));
        }
    }
}