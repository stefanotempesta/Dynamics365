using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Migration
{
    /// <summary>
    /// Custom HTTP message handler that uses OAuth authentication through ADAL.
    /// </summary>
    internal class OAuthMessageHandler : DelegatingHandler
    {
        public OAuthMessageHandler(string serviceUrl, string clientId, string redirectUrl, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            // Obtain the Azure Active Directory Authentication Library (ADAL) authentication context.
            var ap = AuthenticationParameters.CreateFromResourceUrlAsync(new Uri(serviceUrl + "api/data/")).Result;
            AuthenticationContext authContext = new AuthenticationContext(ap.Authority, false);
            
            // Note that an Azure AD access token has finite lifetime, default expiration is 60 minutes.
            AuthenticationResult authResult = authContext.AcquireTokenAsync(serviceUrl, clientId, new Uri(redirectUrl), new PlatformParameters(PromptBehavior.Always)).Result;
            authHeader = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = authHeader;
            return base.SendAsync(request, cancellationToken);
        }

        private AuthenticationHeaderValue authHeader;
    }
}
