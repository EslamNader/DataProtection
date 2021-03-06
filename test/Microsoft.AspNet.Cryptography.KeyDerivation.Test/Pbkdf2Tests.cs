﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.AspNet.Cryptography.KeyDerivation.PBKDF2;
using Microsoft.AspNet.DataProtection.Test.Shared;
using Microsoft.AspNet.Testing.xunit;
using Xunit;

namespace Microsoft.AspNet.Cryptography.KeyDerivation
{
    public class Pbkdf2Tests
    {
        // The 'numBytesRequested' parameters below are chosen to exercise code paths where
        // this value straddles the digest length of the PRF. We only use 5 iterations so
        // that our unit tests are fast.
        [Theory]
        [InlineData("my-password", KeyDerivationPrf.Sha1, 5, 160 / 8 - 1, "efmxNcKD/U1urTEDGvsThlPnHA==")]
        [InlineData("my-password", KeyDerivationPrf.Sha1, 5, 160 / 8 + 0, "efmxNcKD/U1urTEDGvsThlPnHDI=")]
        [InlineData("my-password", KeyDerivationPrf.Sha1, 5, 160 / 8 + 1, "efmxNcKD/U1urTEDGvsThlPnHDLk")]
        [InlineData("my-password", KeyDerivationPrf.Sha256, 5, 256 / 8 - 1, "JRNz8bPKS02EG1vf7eWjA64IeeI+TI8gBEwb1oVvRA==")]
        [InlineData("my-password", KeyDerivationPrf.Sha256, 5, 256 / 8 + 0, "JRNz8bPKS02EG1vf7eWjA64IeeI+TI8gBEwb1oVvRLo=")]
        [InlineData("my-password", KeyDerivationPrf.Sha256, 5, 256 / 8 + 1, "JRNz8bPKS02EG1vf7eWjA64IeeI+TI8gBEwb1oVvRLpk")]
        [InlineData("my-password", KeyDerivationPrf.Sha512, 5, 512 / 8 - 1, "ZTallQJrFn0279xIzaiA1XqatVTGei+ZjKngA7bIMtKMDUw6YJeGUQpFG8iGTgN+ri3LNDktNbzwfcSyZmm9")]
        [InlineData("my-password", KeyDerivationPrf.Sha512, 5, 512 / 8 + 0, "ZTallQJrFn0279xIzaiA1XqatVTGei+ZjKngA7bIMtKMDUw6YJeGUQpFG8iGTgN+ri3LNDktNbzwfcSyZmm90Q==")]
        [InlineData("my-password", KeyDerivationPrf.Sha512, 5, 512 / 8 + 1, "ZTallQJrFn0279xIzaiA1XqatVTGei+ZjKngA7bIMtKMDUw6YJeGUQpFG8iGTgN+ri3LNDktNbzwfcSyZmm90Wk=")]
        public void RunTest_Normal_Managed(string password, KeyDerivationPrf prf, int iterationCount, int numBytesRequested, string expectedValueAsBase64)
        {
            // Arrange
            byte[] salt = new byte[256];
            for (int i = 0; i < salt.Length; i++)
            {
                salt[i] = (byte)i;
            }

            // Act & assert
            TestProvider<ManagedPbkdf2Provider>(password, salt, prf, iterationCount, numBytesRequested, expectedValueAsBase64);
        }

        // The 'numBytesRequested' parameters below are chosen to exercise code paths where
        // this value straddles the digest length of the PRF. We only use 5 iterations so
        // that our unit tests are fast.
        [ConditionalTheory]
        [ConditionalRunTestOnlyOnWindows]
        [InlineData("my-password", KeyDerivationPrf.Sha1, 5, 160 / 8 - 1, "efmxNcKD/U1urTEDGvsThlPnHA==")]
        [InlineData("my-password", KeyDerivationPrf.Sha1, 5, 160 / 8 + 0, "efmxNcKD/U1urTEDGvsThlPnHDI=")]
        [InlineData("my-password", KeyDerivationPrf.Sha1, 5, 160 / 8 + 1, "efmxNcKD/U1urTEDGvsThlPnHDLk")]
        [InlineData("my-password", KeyDerivationPrf.Sha256, 5, 256 / 8 - 1, "JRNz8bPKS02EG1vf7eWjA64IeeI+TI8gBEwb1oVvRA==")]
        [InlineData("my-password", KeyDerivationPrf.Sha256, 5, 256 / 8 + 0, "JRNz8bPKS02EG1vf7eWjA64IeeI+TI8gBEwb1oVvRLo=")]
        [InlineData("my-password", KeyDerivationPrf.Sha256, 5, 256 / 8 + 1, "JRNz8bPKS02EG1vf7eWjA64IeeI+TI8gBEwb1oVvRLpk")]
        [InlineData("my-password", KeyDerivationPrf.Sha512, 5, 512 / 8 - 1, "ZTallQJrFn0279xIzaiA1XqatVTGei+ZjKngA7bIMtKMDUw6YJeGUQpFG8iGTgN+ri3LNDktNbzwfcSyZmm9")]
        [InlineData("my-password", KeyDerivationPrf.Sha512, 5, 512 / 8 + 0, "ZTallQJrFn0279xIzaiA1XqatVTGei+ZjKngA7bIMtKMDUw6YJeGUQpFG8iGTgN+ri3LNDktNbzwfcSyZmm90Q==")]
        [InlineData("my-password", KeyDerivationPrf.Sha512, 5, 512 / 8 + 1, "ZTallQJrFn0279xIzaiA1XqatVTGei+ZjKngA7bIMtKMDUw6YJeGUQpFG8iGTgN+ri3LNDktNbzwfcSyZmm90Wk=")]
        public void RunTest_Normal_Win7(string password, KeyDerivationPrf prf, int iterationCount, int numBytesRequested, string expectedValueAsBase64)
        {
            // Arrange
            byte[] salt = new byte[256];
            for (int i = 0; i < salt.Length; i++)
            {
                salt[i] = (byte)i;
            }

            // Act & assert
            TestProvider<Win7Pbkdf2Provider>(password, salt, prf, iterationCount, numBytesRequested, expectedValueAsBase64);
        }

        // The 'numBytesRequested' parameters below are chosen to exercise code paths where
        // this value straddles the digest length of the PRF. We only use 5 iterations so
        // that our unit tests are fast.
        [ConditionalTheory]
        [ConditionalRunTestOnlyOnWindows8OrLater]
        [InlineData("my-password", KeyDerivationPrf.Sha1, 5, 160 / 8 - 1, "efmxNcKD/U1urTEDGvsThlPnHA==")]
        [InlineData("my-password", KeyDerivationPrf.Sha1, 5, 160 / 8 + 0, "efmxNcKD/U1urTEDGvsThlPnHDI=")]
        [InlineData("my-password", KeyDerivationPrf.Sha1, 5, 160 / 8 + 1, "efmxNcKD/U1urTEDGvsThlPnHDLk")]
        [InlineData("my-password", KeyDerivationPrf.Sha256, 5, 256 / 8 - 1, "JRNz8bPKS02EG1vf7eWjA64IeeI+TI8gBEwb1oVvRA==")]
        [InlineData("my-password", KeyDerivationPrf.Sha256, 5, 256 / 8 + 0, "JRNz8bPKS02EG1vf7eWjA64IeeI+TI8gBEwb1oVvRLo=")]
        [InlineData("my-password", KeyDerivationPrf.Sha256, 5, 256 / 8 + 1, "JRNz8bPKS02EG1vf7eWjA64IeeI+TI8gBEwb1oVvRLpk")]
        [InlineData("my-password", KeyDerivationPrf.Sha512, 5, 512 / 8 - 1, "ZTallQJrFn0279xIzaiA1XqatVTGei+ZjKngA7bIMtKMDUw6YJeGUQpFG8iGTgN+ri3LNDktNbzwfcSyZmm9")]
        [InlineData("my-password", KeyDerivationPrf.Sha512, 5, 512 / 8 + 0, "ZTallQJrFn0279xIzaiA1XqatVTGei+ZjKngA7bIMtKMDUw6YJeGUQpFG8iGTgN+ri3LNDktNbzwfcSyZmm90Q==")]
        [InlineData("my-password", KeyDerivationPrf.Sha512, 5, 512 / 8 + 1, "ZTallQJrFn0279xIzaiA1XqatVTGei+ZjKngA7bIMtKMDUw6YJeGUQpFG8iGTgN+ri3LNDktNbzwfcSyZmm90Wk=")]
        public void RunTest_Normal_Win8(string password, KeyDerivationPrf prf, int iterationCount, int numBytesRequested, string expectedValueAsBase64)
        {
            // Arrange
            byte[] salt = new byte[256];
            for (int i = 0; i < salt.Length; i++)
            {
                salt[i] = (byte)i;
            }

            // Act & assert
            TestProvider<Win8Pbkdf2Provider>(password, salt, prf, iterationCount, numBytesRequested, expectedValueAsBase64);
        }

        [Fact]
        public void RunTest_WithLongPassword_Managed()
        {
            RunTest_WithLongPassword_Impl<ManagedPbkdf2Provider>();
        }

        [ConditionalFact]
        [ConditionalRunTestOnlyOnWindows]
        public void RunTest_WithLongPassword_Win7()
        {
            RunTest_WithLongPassword_Impl<Win7Pbkdf2Provider>();
        }

        [ConditionalFact]
        [ConditionalRunTestOnlyOnWindows8OrLater]
        public void RunTest_WithLongPassword_Win8()
        {
            RunTest_WithLongPassword_Impl<Win8Pbkdf2Provider>();
        }

        private static void RunTest_WithLongPassword_Impl<TProvider>()
            where TProvider : IPbkdf2Provider, new()
        {
            // Arrange
            string password = new String('x', 50000); // 50,000 char password
            byte[] salt = Encoding.UTF8.GetBytes("salt");
            const string expectedDerivedKeyBase64 = "Sc+V/c3fiZq5Z5qH3iavAiojTsW97FAp2eBNmCQAwCNzA8hfhFFYyQLIMK65qPnBFHOHXQPwAxNQNhaEAH9hzfiaNBSRJpF9V4rpl02d5ZpI6cZbsQFF7TJW7XJzQVpYoPDgJlg0xVmYLhn1E9qMtUVUuXsBjOOdd7K1M+ZI00c=";
            const KeyDerivationPrf prf = KeyDerivationPrf.Sha256;
            const int iterationCount = 5;
            const int numBytesRequested = 128;

            // Act & assert
            TestProvider<TProvider>(password, salt, prf, iterationCount, numBytesRequested, expectedDerivedKeyBase64);
        }

        private static void TestProvider<TProvider>(string password, byte[] salt, KeyDerivationPrf prf, int iterationCount, int numBytesRequested, string expectedDerivedKeyAsBase64)
            where TProvider : IPbkdf2Provider, new()
        {
            byte[] derivedKey = new TProvider().DeriveKey(password, salt, prf, iterationCount, numBytesRequested);
            Assert.Equal(numBytesRequested, derivedKey.Length);
            Assert.Equal(expectedDerivedKeyAsBase64, Convert.ToBase64String(derivedKey));
        }
    }
}
