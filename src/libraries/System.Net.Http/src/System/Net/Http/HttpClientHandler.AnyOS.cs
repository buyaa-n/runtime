// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Security;
using System.Runtime.Versioning;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace System.Net.Http
{
    public partial class HttpClientHandler
    {
        private readonly SocketsHttpHandler _underlyingHandler;

        public HttpClientHandler()
        {
            _underlyingHandler = new SocketsHttpHandler();
            if (DiagnosticsHandler.IsGloballyEnabled())
            {
                _diagnosticsHandler = new DiagnosticsHandler(_underlyingHandler);
            }
            ClientCertificateOptions = ClientCertificateOption.Manual;
        }

        public virtual bool SupportsAutomaticDecompression => SocketsHttpHandler.SupportsAutomaticDecompression;
        public virtual bool SupportsProxy => SocketsHttpHandler.SupportsProxy;
        public virtual bool SupportsRedirectConfiguration => SocketsHttpHandler.SupportsRedirectConfiguration;

        [UnsupportedOSPlatform("browser")]
        public bool UseCookies
        {
            get => _underlyingHandler.UseCookies;
            set => _underlyingHandler.UseCookies = value;
        }

        [UnsupportedOSPlatform("browser")]
        public CookieContainer CookieContainer
        {
            get => _underlyingHandler.CookieContainer;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _underlyingHandler.CookieContainer = value;
            }
        }

        [UnsupportedOSPlatform("browser")]
        public DecompressionMethods AutomaticDecompression
        {
            get => _underlyingHandler.AutomaticDecompression;
            set => _underlyingHandler.AutomaticDecompression = value;
        }

        [UnsupportedOSPlatform("browser")]
        public bool UseProxy
        {
            get => _underlyingHandler.UseProxy;
            set => _underlyingHandler.UseProxy = value;
        }

        [UnsupportedOSPlatform("browser")]
        public IWebProxy? Proxy
        {
            get => _underlyingHandler.Proxy;
            set => _underlyingHandler.Proxy = value;
        }

        [UnsupportedOSPlatform("browser")]
        public ICredentials? DefaultProxyCredentials
        {
            get => _underlyingHandler.DefaultProxyCredentials;
            set => _underlyingHandler.DefaultProxyCredentials = value;
        }

        [UnsupportedOSPlatform("browser")]
        public bool PreAuthenticate
        {
            get => _underlyingHandler.PreAuthenticate;
            set => _underlyingHandler.PreAuthenticate = value;
        }

        [UnsupportedOSPlatform("browser")]
        public bool UseDefaultCredentials
        {
            // SocketsHttpHandler doesn't have a separate UseDefaultCredentials property.  There
            // is just a Credentials property.  So, we need to map the behavior.
            get => _underlyingHandler.Credentials == CredentialCache.DefaultCredentials;
            set
            {
                if (value)
                {
                    _underlyingHandler.Credentials = CredentialCache.DefaultCredentials;
                }
                else
                {
                    if (_underlyingHandler.Credentials == CredentialCache.DefaultCredentials)
                    {
                        // Only clear out the Credentials property if it was a DefaultCredentials.
                        _underlyingHandler.Credentials = null;
                    }
                }
            }
        }

        [UnsupportedOSPlatform("browser")]
        public ICredentials? Credentials
        {
            get => _underlyingHandler.Credentials;
            set => _underlyingHandler.Credentials = value;
        }

        [UnsupportedOSPlatform("browser")]
        public int MaxAutomaticRedirections
        {
            get => _underlyingHandler.MaxAutomaticRedirections;
            set => _underlyingHandler.MaxAutomaticRedirections = value;
        }

        [UnsupportedOSPlatform("browser")]
        public int MaxConnectionsPerServer
        {
            get => _underlyingHandler.MaxConnectionsPerServer;
            set => _underlyingHandler.MaxConnectionsPerServer = value;
        }

        [UnsupportedOSPlatform("browser")]
        public int MaxResponseHeadersLength
        {
            get => _underlyingHandler.MaxResponseHeadersLength;
            set => _underlyingHandler.MaxResponseHeadersLength = value;
        }

        public ClientCertificateOption ClientCertificateOptions
        {
            get => _clientCertificateOptions;
            set
            {
                switch (value)
                {
                    case ClientCertificateOption.Manual:
                        ThrowForModifiedManagedSslOptionsIfStarted();
                        _clientCertificateOptions = value;
                        _underlyingHandler.SslOptions.LocalCertificateSelectionCallback = (sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers) => CertificateHelper.GetEligibleClientCertificate(ClientCertificates)!;
                        break;

                    case ClientCertificateOption.Automatic:
                        ThrowForModifiedManagedSslOptionsIfStarted();
                        _clientCertificateOptions = value;
                        _underlyingHandler.SslOptions.LocalCertificateSelectionCallback = (sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers) => CertificateHelper.GetEligibleClientCertificate()!;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        [UnsupportedOSPlatform("browser")]
        public X509CertificateCollection ClientCertificates
        {
            get
            {
                if (ClientCertificateOptions != ClientCertificateOption.Manual)
                {
                    throw new InvalidOperationException(SR.Format(SR.net_http_invalid_enable_first, nameof(ClientCertificateOptions), nameof(ClientCertificateOption.Manual)));
                }

                return _underlyingHandler.SslOptions.ClientCertificates ??
                    (_underlyingHandler.SslOptions.ClientCertificates = new X509CertificateCollection());
            }
        }

        [UnsupportedOSPlatform("browser")]
        public Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool>? ServerCertificateCustomValidationCallback
        {
            get => (_underlyingHandler.SslOptions.RemoteCertificateValidationCallback?.Target as ConnectHelper.CertificateCallbackMapper)?.FromHttpClientHandler;
            set
            {
                ThrowForModifiedManagedSslOptionsIfStarted();
                _underlyingHandler.SslOptions.RemoteCertificateValidationCallback = value != null ?
                    new ConnectHelper.CertificateCallbackMapper(value).ForSocketsHttpHandler :
                    null;
            }
        }

        [UnsupportedOSPlatform("browser")]
        public bool CheckCertificateRevocationList
        {
            get => _underlyingHandler.SslOptions.CertificateRevocationCheckMode == X509RevocationMode.Online;
            set
            {
                ThrowForModifiedManagedSslOptionsIfStarted();
                _underlyingHandler.SslOptions.CertificateRevocationCheckMode = value ? X509RevocationMode.Online : X509RevocationMode.NoCheck;
            }
        }

        [UnsupportedOSPlatform("browser")]
        public SslProtocols SslProtocols
        {
            get => _underlyingHandler.SslOptions.EnabledSslProtocols;
            set
            {
                ThrowForModifiedManagedSslOptionsIfStarted();
                _underlyingHandler.SslOptions.EnabledSslProtocols = value;
            }
        }

        [UnsupportedOSPlatform("browser")]
        protected internal override HttpResponseMessage Send(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return DiagnosticsHandler.IsEnabled() && _diagnosticsHandler != null ?
                _diagnosticsHandler.Send(request, cancellationToken) :
                _underlyingHandler.Send(request, cancellationToken);
        }

        // lazy-load the validator func so it can be trimmed by the ILLinker if it isn't used.
        private static Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool>? s_dangerousAcceptAnyServerCertificateValidator;

        [UnsupportedOSPlatform("browser")]
        public static Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool> DangerousAcceptAnyServerCertificateValidator =>
            Volatile.Read(ref s_dangerousAcceptAnyServerCertificateValidator) ??
            Interlocked.CompareExchange(ref s_dangerousAcceptAnyServerCertificateValidator, delegate { return true; }, null) ??
            s_dangerousAcceptAnyServerCertificateValidator;
    }
}
