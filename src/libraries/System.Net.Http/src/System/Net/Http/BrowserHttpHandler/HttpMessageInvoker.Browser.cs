// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Versioning;
using System.Threading;

namespace System.Net.Http
{
    public partial class HttpMessageInvoker
    {
        [UnsupportedOSPlatform("browser")]
        public virtual HttpResponseMessage Send(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
