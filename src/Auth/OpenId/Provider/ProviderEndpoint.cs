using System;
using System.Web;

namespace Kiss.Web.Auth.OpenId.Provider
{

    /// <summary>
    /// An OpenID Provider control that automatically responds to certain
    /// automated OpenID messages, and routes authentication requests to
    /// custom code via an event handler.
    /// </summary>
    public class ProviderEndpoint
    {
        /// <summary>
        /// The key used to store the pending authentication request in the ASP.NET session.
        /// </summary>
        private const string PendingRequestKey = "pendingRequest";

        /// <summary>
        /// Backing field for the <see cref="Provider"/> property.
        /// </summary>
        private static OpenIdProvider provider;

        /// <summary>
        /// The lock that must be obtained when initializing the provider field.
        /// </summary>
        private static readonly object providerInitializerLock = new object();

        /// <summary>
        /// Fired when an incoming OpenID request is an authentication challenge
        /// that must be responded to by the Provider web site according to its
        /// own user database and policies.
        /// </summary>
        public event EventHandler<AuthenticationChallengeEventArgs> AuthenticationChallenge;

        /// <summary>
        /// Fired when an incoming OpenID message carries extension requests
        /// but is not regarding any OpenID identifier.
        /// </summary>
        public event EventHandler<AnonymousRequestEventArgs> AnonymousRequest;

        /// <summary>
        /// Gets or sets the <see cref="OpenIdProvider"/> instance to use for all instances of this control.
        /// </summary>
        /// <value>The default value is an <see cref="OpenIdProvider"/> instance initialized according to the web.config file.</value>
        public static OpenIdProvider Provider
        {
            get
            {
                if (provider == null)
                {
                    lock (providerInitializerLock)
                    {
                        if (provider == null)
                        {
                            provider = CreateProvider();
                        }
                    }
                }

                return provider;
            }

            set
            {
                provider = value;
            }
        }

        /// <summary>
        /// Gets or sets an incoming OpenID authentication request that has not yet been responded to.
        /// </summary>
        /// <remarks>
        /// This request is stored in the ASP.NET Session state, so it will survive across
        /// redirects, postbacks, and transfers.  This allows you to authenticate the user
        /// yourself, and confirm his/her desire to authenticate to the relying party site
        /// before responding to the relying party's authentication request.
        /// </remarks>
        public static IAuthenticationRequest PendingAuthenticationRequest
        {
            get
            {
                return HttpContext.Current.Session[PendingRequestKey] as IAuthenticationRequest;
            }

            set
            {
                HttpContext.Current.Session[PendingRequestKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets an incoming OpenID anonymous request that has not yet been responded to.
        /// </summary>
        /// <remarks>
        /// This request is stored in the ASP.NET Session state, so it will survive across
        /// redirects, postbacks, and transfers.  This allows you to authenticate the user
        /// yourself, and confirm his/her desire to provide data to the relying party site
        /// before responding to the relying party's request.
        /// </remarks>
        public static IAnonymousRequest PendingAnonymousRequest
        {
            get
            {
                return HttpContext.Current.Session[PendingRequestKey] as IAnonymousRequest;
            }

            set
            {
                HttpContext.Current.Session[PendingRequestKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets an incoming OpenID request that has not yet been responded to.
        /// </summary>
        /// <remarks>
        /// This request is stored in the ASP.NET Session state, so it will survive across
        /// redirects, postbacks, and transfers.  This allows you to authenticate the user
        /// yourself, and confirm his/her desire to provide data to the relying party site
        /// before responding to the relying party's request.
        /// </remarks>
        public static IHostProcessedRequest PendingRequest
        {
            get
            {
                return HttpContext.Current.Session[PendingRequestKey] as IHostProcessedRequest;
            }

            set
            {
                HttpContext.Current.Session[PendingRequestKey] = value;
            }
        }

        /// <summary>
        /// Sends the response for the <see cref="PendingAuthenticationRequest"/> and clears the property.
        /// </summary>
        public static void SendResponse()
        {
            var pendingRequest = PendingRequest;
            PendingRequest = null;
            Provider.SendResponse(pendingRequest);
        }

        public void GetRequest()
        {
            // Use the explicitly given state store on this control if there is one.  
            // Then try the configuration file specified one.  Finally, use the default
            // in-memory one that's built into OpenIdProvider.
            // determine what incoming message was received
            IRequest request = Provider.GetRequest();
            if (request != null)
            {
                PendingRequest = null;

                // process the incoming message appropriately and send the response
                IAuthenticationRequest idrequest;
                IAnonymousRequest anonRequest;
                if ((idrequest = request as IAuthenticationRequest) != null)
                {
                    PendingAuthenticationRequest = idrequest;
                    OnAuthenticationChallenge(idrequest);
                }
                else if ((anonRequest = request as IAnonymousRequest) != null)
                {
                    PendingAnonymousRequest = anonRequest;
                    if (!OnAnonymousRequest(anonRequest))
                    {
                        anonRequest.IsApproved = false;
                    }
                }

                if (request.IsResponseReady)
                {
                    Provider.SendResponse(request);

                    HttpContext.Current.Response.End();

                    PendingAuthenticationRequest = null;
                }
            }
        }

        /// <summary>
        /// Fires the <see cref="AuthenticationChallenge"/> event.
        /// </summary>
        /// <param name="request">The request to include in the event args.</param>
        protected virtual void OnAuthenticationChallenge(IAuthenticationRequest request)
        {
            var authenticationChallenge = this.AuthenticationChallenge;
            if (authenticationChallenge != null)
            {
                authenticationChallenge(this, new AuthenticationChallengeEventArgs(request));
            }
        }

        /// <summary>
        /// Fires the <see cref="AnonymousRequest"/> event.
        /// </summary>
        /// <param name="request">The request to include in the event args.</param>
        /// <returns><c>true</c> if there were any anonymous request handlers.</returns>
        protected virtual bool OnAnonymousRequest(IAnonymousRequest request)
        {
            var anonymousRequest = this.AnonymousRequest;
            if (anonymousRequest != null)
            {
                anonymousRequest(this, new AnonymousRequestEventArgs(request));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Creates the default OpenIdProvider to use.
        /// </summary>
        /// <returns>The new instance of OpenIdProvider.</returns>
        private static OpenIdProvider CreateProvider()
        {
            return new OpenIdProvider();
        }
    }
}
