// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public partial class HttpClient
    {

        [UnsupportedOSPlatform("browser")]
        public HttpResponseMessage Send(HttpRequestMessage request) =>
            Send(request, DefaultCompletionOption, cancellationToken: default);

        [UnsupportedOSPlatform("browser")]
        public HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption) =>
            Send(request, completionOption, cancellationToken: default);

        [UnsupportedOSPlatform("browser")]
        public override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Send(request, DefaultCompletionOption, cancellationToken);

        [UnsupportedOSPlatform("browser")]
        public HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            // Called outside of async state machine to propagate certain exception even without awaiting the returned task.
            CheckRequestBeforeSend(request);

            (CancellationTokenSource cts, bool disposeCts, CancellationTokenSource pendingRequestsCts) = PrepareCancellationTokenSource(cancellationToken);
            ValueTask<HttpResponseMessage> sendTask = SendAsyncCore(request, completionOption, async: false, cts, disposeCts, pendingRequestsCts, cancellationToken);
            Debug.Assert(sendTask.IsCompleted);
            return sendTask.GetAwaiter().GetResult();
        }
    }
}
