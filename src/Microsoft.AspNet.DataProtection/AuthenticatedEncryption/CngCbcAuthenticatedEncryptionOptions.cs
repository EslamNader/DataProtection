﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Cryptography;
using Microsoft.AspNet.Cryptography.Cng;
using Microsoft.AspNet.Cryptography.SafeHandles;
using Microsoft.AspNet.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNet.DataProtection.Cng;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.DataProtection.AuthenticatedEncryption
{
    /// <summary>
    /// Options for configuring an authenticated encryption mechanism which uses
    /// Windows CNG algorithms in CBC encryption + HMAC authentication modes.
    /// </summary>
    public sealed class CngCbcAuthenticatedEncryptionOptions : IInternalAuthenticatedEncryptionOptions
    {
        /// <summary>
        /// The name of the algorithm to use for symmetric encryption.
        /// This property corresponds to the 'pszAlgId' parameter of BCryptOpenAlgorithmProvider.
        /// This property is required to have a value.
        /// </summary>
        /// <remarks>
        /// The algorithm must support CBC-style encryption and must have a block size of 64 bits
        /// or greater.
        /// The default value is 'AES'.
        /// </remarks>
        [ApplyPolicy]
        public string EncryptionAlgorithm { get; set; } = Constants.BCRYPT_AES_ALGORITHM;

        /// <summary>
        /// The name of the provider which contains the implementation of the symmetric encryption algorithm.
        /// This property corresponds to the 'pszImplementation' parameter of BCryptOpenAlgorithmProvider.
        /// This property is optional.
        /// </summary>
        /// <remarks>
        /// The default value is null.
        /// </remarks>
        [ApplyPolicy]
        public string EncryptionAlgorithmProvider { get; set; } = null;

        /// <summary>
        /// The length (in bits) of the key that will be used for symmetric encryption.
        /// This property is required to have a value.
        /// </summary>
        /// <remarks>
        /// The key length must be 128 bits or greater.
        /// The default value is 256.
        /// </remarks>
        [ApplyPolicy]
        public int EncryptionAlgorithmKeySize { get; set; } = 256;

        /// <summary>
        /// The name of the algorithm to use for hashing data.
        /// This property corresponds to the 'pszAlgId' parameter of BCryptOpenAlgorithmProvider.
        /// This property is required to have a value.
        /// </summary>
        /// <remarks>
        /// The algorithm must support being opened in HMAC mode and must have a digest length
        /// of 128 bits or greater.
        /// The default value is 'SHA256'.
        /// </remarks>
        [ApplyPolicy]
        public string HashAlgorithm { get; set; } = Constants.BCRYPT_SHA256_ALGORITHM;

        /// <summary>
        /// The name of the provider which contains the implementation of the hash algorithm.
        /// This property corresponds to the 'pszImplementation' parameter of BCryptOpenAlgorithmProvider.
        /// This property is optional.
        /// </summary>
        /// <remarks>
        /// The default value is null.
        /// </remarks>
        [ApplyPolicy]
        public string HashAlgorithmProvider { get; set; } = null;

        /// <summary>
        /// Validates that this <see cref="CngCbcAuthenticatedEncryptionOptions"/> is well-formed, i.e.,
        /// that the specified algorithms actually exist and that they can be instantiated properly.
        /// An exception will be thrown if validation fails.
        /// </summary>
        public void Validate()
        {
            // Run a sample payload through an encrypt -> decrypt operation to make sure data round-trips properly.
            using (var encryptor = CreateAuthenticatedEncryptorInstance(Secret.Random(512 / 8)))
            {
                encryptor.PerformSelfTest();
            }
        }

        /*
         * HELPER ROUTINES
         */

        internal CbcAuthenticatedEncryptor CreateAuthenticatedEncryptorInstance(ISecret secret, ILogger logger = null)
        {
            return new CbcAuthenticatedEncryptor(
                keyDerivationKey: new Secret(secret),
                symmetricAlgorithmHandle: GetSymmetricBlockCipherAlgorithmHandle(logger),
                symmetricAlgorithmKeySizeInBytes: (uint)(EncryptionAlgorithmKeySize / 8),
                hmacAlgorithmHandle: GetHmacAlgorithmHandle(logger));
        }

        private BCryptAlgorithmHandle GetHmacAlgorithmHandle(ILogger logger)
        {
            // basic argument checking
            if (String.IsNullOrEmpty(HashAlgorithm))
            {
                throw Error.Common_PropertyCannotBeNullOrEmpty(nameof(HashAlgorithm));
            }

            if (logger.IsVerboseLevelEnabled())
            {
                logger.LogVerbose("Opening CNG algorithm '{0}' from provider '{1}' with HMAC.", HashAlgorithm, HashAlgorithmProvider);
            }

            BCryptAlgorithmHandle algorithmHandle = null;

            // Special-case cached providers
            if (HashAlgorithmProvider == null)
            {
                if (HashAlgorithm == Constants.BCRYPT_SHA1_ALGORITHM) { algorithmHandle = CachedAlgorithmHandles.HMAC_SHA1; }
                else if (HashAlgorithm == Constants.BCRYPT_SHA256_ALGORITHM) { algorithmHandle = CachedAlgorithmHandles.HMAC_SHA256; }
                else if (HashAlgorithm == Constants.BCRYPT_SHA512_ALGORITHM) { algorithmHandle = CachedAlgorithmHandles.HMAC_SHA512; }
            }

            // Look up the provider dynamically if we couldn't fetch a cached instance
            if (algorithmHandle == null)
            {
                algorithmHandle = BCryptAlgorithmHandle.OpenAlgorithmHandle(HashAlgorithm, HashAlgorithmProvider, hmac: true);
            }

            // Make sure we're using a hash algorithm. We require a minimum 128-bit digest.
            uint digestSize = algorithmHandle.GetHashDigestLength();
            AlgorithmAssert.IsAllowableValidationAlgorithmDigestSize(checked(digestSize * 8));

            // all good!
            return algorithmHandle;
        }

        private BCryptAlgorithmHandle GetSymmetricBlockCipherAlgorithmHandle(ILogger logger)
        {
            // basic argument checking
            if (String.IsNullOrEmpty(EncryptionAlgorithm))
            {
                throw Error.Common_PropertyCannotBeNullOrEmpty(nameof(EncryptionAlgorithm));
            }
            if (EncryptionAlgorithmKeySize < 0)
            {
                throw Error.Common_PropertyMustBeNonNegative(nameof(EncryptionAlgorithmKeySize));
            }

            if (logger.IsVerboseLevelEnabled())
            {
                logger.LogVerbose("Opening CNG algorithm '{0}' from provider '{1}' with chaining mode CBC.", EncryptionAlgorithm, EncryptionAlgorithmProvider);
            }

            BCryptAlgorithmHandle algorithmHandle = null;

            // Special-case cached providers
            if (EncryptionAlgorithmProvider == null)
            {
                if (EncryptionAlgorithm == Constants.BCRYPT_AES_ALGORITHM) { algorithmHandle = CachedAlgorithmHandles.AES_CBC; }
            }

            // Look up the provider dynamically if we couldn't fetch a cached instance
            if (algorithmHandle == null)
            {
                algorithmHandle = BCryptAlgorithmHandle.OpenAlgorithmHandle(EncryptionAlgorithm, EncryptionAlgorithmProvider);
                algorithmHandle.SetChainingMode(Constants.BCRYPT_CHAIN_MODE_CBC);
            }

            // make sure we're using a block cipher with an appropriate key size & block size
            AlgorithmAssert.IsAllowableSymmetricAlgorithmBlockSize(checked(algorithmHandle.GetCipherBlockLength() * 8));
            AlgorithmAssert.IsAllowableSymmetricAlgorithmKeySize(checked((uint)EncryptionAlgorithmKeySize));

            // make sure the provided key length is valid
            algorithmHandle.GetSupportedKeyLengths().EnsureValidKeyLength((uint)EncryptionAlgorithmKeySize);

            // all good!
            return algorithmHandle;
        }

        IInternalAuthenticatedEncryptorConfiguration IInternalAuthenticatedEncryptionOptions.ToConfiguration(IServiceProvider services)
        {
            return new CngCbcAuthenticatedEncryptorConfiguration(this, services);
        }
    }
}
