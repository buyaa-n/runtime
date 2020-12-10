// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Security;
using System.Runtime.Versioning;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using HttpHandlerType = System.Net.Http.BrowserHttpHandler;

namespace System.Net.Http
{
    public partial class HttpClientHandler
    {
        private readonly HttpHandlerType _underlyingHandler;

        public HttpClientHandler()
        {
            _underlyingHandler = new HttpHandlerType();
            if (DiagnosticsHandler.IsGloballyEnabled())
            {
                _diagnosticsHandler = new DiagnosticsHandler(_underlyingHandler);
            }
            ClientCertificateOptions = ClientCertificateOption.Manual;
        }

        public virtual bool SupportsAutomaticDecompression => HttpHandlerType.SupportsAutomaticDecompression;
        public virtual bool SupportsProxy => HttpHandlerType.SupportsProxy;
        public virtual bool SupportsRedirectConfiguration => HttpHandlerType.SupportsRedirectConfiguration;

        [UnsupportedOSPlatform("browser")]
        public bool UseCookies
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public CookieContainer CookieContainer
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public DecompressionMethods AutomaticDecompression
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public bool UseProxy
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public IWebProxy? Proxy
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public ICredentials? DefaultProxyCredentials
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public bool PreAuthenticate
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public bool UseDefaultCredentials
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public ICredentials? Credentials
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public int MaxAutomaticRedirections
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public int MaxConnectionsPerServer
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public int MaxResponseHeadersLength
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        public ClientCertificateOption ClientCertificateOptions
        {
            get => _clientCertificateOptions;
            set
            {
                switch (value)
                {
                    case ClientCertificateOption.Manual:
                        _clientCertificateOptions = value;
                        break;

                    case ClientCertificateOption.Automatic:
                        _clientCertificateOptions = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        [UnsupportedOSPlatform("browser")]
        public X509CertificateCollection ClientCertificates => throw new PlatformNotSupportedException();

        [UnsupportedOSPlatform("browser")]
        public Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool>? ServerCertificateCustomValidationCallback
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public bool CheckCertificateRevocationList
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        public SslProtocols SslProtocols
        {
            get => throw new PlatformNotSupportedException();
            set { throw new PlatformNotSupportedException(); }
        }

        [UnsupportedOSPlatform("browser")]
        protected internal override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new PlatformNotSupportedException();

        [UnsupportedOSPlatform("browser")]
        public static Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool> DangerousAcceptAnyServerCertificateValidator
            => throw new PlatformNotSupportedException();
    }
}
