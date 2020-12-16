// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Versioning;
using System.Threading;

namespace System.Net.Http
{
    public partial class HttpClient
    {
        [UnsupportedOSPlatform("browser")]
        public HttpResponseMessage Send(HttpRequestMessage request) => throw new PlatformNotSupportedException();

        [UnsupportedOSPlatform("browser")]
        public HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption) =>
            throw new PlatformNotSupportedException();

        [UnsupportedOSPlatform("browser")]
        public override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new PlatformNotSupportedException();

        [UnsupportedOSPlatform("browser")]
        public HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken) =>
            throw new PlatformNotSupportedException();
    }
}
