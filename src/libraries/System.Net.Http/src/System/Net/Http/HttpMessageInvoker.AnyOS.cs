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
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            CheckDisposed();

            if (HttpTelemetry.Log.IsEnabled() && !request.WasSentByHttpClient() && request.RequestUri != null)
            {
                HttpTelemetry.Log.RequestStart(request);

                try
                {
                    return _handler.Send(request, cancellationToken);
                }
                catch when (LogRequestFailed(telemetryStarted: true))
                {
                    // Unreachable as LogRequestFailed will return false
                    throw;
                }
                finally
                {
                    HttpTelemetry.Log.RequestStop();
                }
            }
            else
            {
                return _handler.Send(request, cancellationToken);
            }
        }
    }
}
