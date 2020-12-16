// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Versioning;

namespace System.Security.Cryptography
{
    public sealed partial class IncrementalHash
    {
        /// <summary>
        /// Not suuported in Browser Web Assembly
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">When used in browser target</exception>
        [UnsupportedOSPlatform("browser")]
        public static IncrementalHash CreateHMAC(HashAlgorithmName hashAlgorithm, ReadOnlySpan<byte> key) => throw new PlatformNotSupportedException();
    }
}
