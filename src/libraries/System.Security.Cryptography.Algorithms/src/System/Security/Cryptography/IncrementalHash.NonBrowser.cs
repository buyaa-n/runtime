// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Versioning;
using Internal.Cryptography;

namespace System.Security.Cryptography
{
    public sealed partial class IncrementalHash
    {
        /// <summary>
        /// Create an <see cref="IncrementalHash"/> for the Hash-based Message Authentication Code (HMAC)
        /// algorithm utilizing the hash algorithm specified by <paramref name="hashAlgorithm"/>, and a
        /// key specified by <paramref name="key"/>.
        /// </summary>
        /// <param name="hashAlgorithm">The name of the hash algorithm to perform within the HMAC.</param>
        /// <param name="key">
        ///     The secret key for the HMAC. The key can be any length, but a key longer than the output size
        ///     of the hash algorithm specified by <paramref name="hashAlgorithm"/> will be hashed (using the
        ///     algorithm specified by <paramref name="hashAlgorithm"/>) to derive a correctly-sized key. Therefore,
        ///     the recommended size of the secret key is the output size of the hash specified by
        ///     <paramref name="hashAlgorithm"/>.
        /// </param>
        /// <returns>
        /// An <see cref="IncrementalHash"/> instance ready to compute the hash algorithm specified
        /// by <paramref name="hashAlgorithm"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     <paramref name="hashAlgorithm"/>.<see cref="HashAlgorithmName.Name"/> is <c>null</c>, or
        ///     the empty string.
        /// </exception>
        /// <exception cref="CryptographicException"><paramref name="hashAlgorithm"/> is not a known hash algorithm.</exception>
        [UnsupportedOSPlatform("browser")]
        public static IncrementalHash CreateHMAC(HashAlgorithmName hashAlgorithm, ReadOnlySpan<byte> key)
        {
            if (string.IsNullOrEmpty(hashAlgorithm.Name))
                throw new ArgumentException(SR.Cryptography_HashAlgorithmNameNullOrEmpty, nameof(hashAlgorithm));

            return new IncrementalHash(hashAlgorithm, new HMACCommon(hashAlgorithm.Name, key, -1));
        }
    }
}
