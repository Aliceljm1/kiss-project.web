using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web;
using Kiss.Plugin;
using Kiss.Web.WebDAV.BaseClasses;
using Kiss.Web.WebDAV.Classes;
using Kiss.Web.WebDAV.Security;

namespace Kiss.Web.WebDAV
{
	/// <summary>
	/// WebDAV Http Module
	/// </summary>
	public abstract class WebDAVInitializer : IPluginInitializer
	{
		/// <summary>
		/// WebDAV Http Module
		/// </summary>
		/// <param name="sourceAssembly">
		/// Assembly containing the base class implementations
		/// </param>
		public WebDAVInitializer(Assembly sourceAssembly) : this(sourceAssembly, Authentication.None) { }

		/// <summary>
		/// WebDAV Http Module
		/// </summary>
		/// <param name="sourceAssembly">
		/// Assembly containing the base class implementations
		/// </param>
		/// <param name="moduleAuthentication">
		/// Authentication to use during requests
		/// </param>
		public WebDAVInitializer(Assembly sourceAssembly, Authentication moduleAuthentication)
		{
			this.ModuleAuthentication = moduleAuthentication;
			this.WebDavProcessor = new WebDavProcessor(sourceAssembly);
		}

		#region events
		/// <summary>
		/// Occurs when the module is going to process a request
		/// </summary>
		protected event EventHandler<DavProcessRequestArgs> ProcessRequest;

		/// <summary>
		/// Raises the ProcessRequest event
		/// </summary>
		private void OnProcessRequest(DavProcessRequestArgs e)
		{
			if (ProcessRequest != null)
				ProcessRequest(this, e);
		}

		/// <summary>
		/// Occurs when the module has finished processing a request
		/// </summary>
		protected event EventHandler RequestProcessed;

		/// <summary>
		/// Raises the ProcessRequest event
		/// </summary>
		private void OnRequestProcessed()
		{
			if (RequestProcessed != null)
				RequestProcessed(this, new EventArgs());
		}

		/// <summary>
		/// Occurs when the module is authenticating a request
		/// </summary>
		protected event EventHandler<AuthenticationArgs> AuthenticateRequest;

		/// <summary>
		/// Raises the Authenticate event
		/// </summary>
		private void OnAuthenticateRequest(AuthenticationArgs e)
		{
			if (AuthenticateRequest != null)
				AuthenticateRequest(this, e);
		}

		/// <summary>
		/// Occurs when the module is authenticating a request
		/// </summary>
		protected event EventHandler<AuthorizationArgs> AuthorizationComplete;

		/// <summary>
		/// Raises the Authorization complete event
		/// </summary>
		private void OnAuthorizationComplete(AuthorizationArgs e)
		{
			if (AuthorizationComplete != null)
				AuthorizationComplete(this, e);
		}

		/// <summary>
		/// Occurs when the module is processing a Basic Auth request
		/// </summary>
		protected event EventHandler<BasicAuthorizationArgs> BasicAuthorization;

		/// <summary>
		/// Raises the BasicAuthorization event
		/// </summary>
		private void OnBasicAuthorization(BasicAuthorizationArgs e)
		{
			if (BasicAuthorization != null)
				BasicAuthorization(this, e);
		}

		/// <summary>
		/// Occurs when the module is processing a Digest Auth request
		/// </summary>
		protected event EventHandler<DigestAuthorizationArgs> DigestAuthorization;

		/// <summary>
		/// Raises the DigestAuthorization event
		/// </summary>
		private void OnDigestAuthorization(DigestAuthorizationArgs e)
		{
			if (DigestAuthorization != null)
				DigestAuthorization(this, e);
		}
		#endregion

		#region props
		/// <summary>
		/// Is WebDAV request
		/// </summary>
		protected bool IsWebDAVRequest { get; set; }

		/// <summary>
		/// Requested HttpMethod
		/// </summary>
		protected string RequestHttpMethod { get; set; }
		#endregion

		#region Internal Properties
		/// <summary>
		/// Authentication type
		/// </summary>
		private Authentication ModuleAuthentication { get; set; }

		private WebDavProcessor WebDavProcessor { get; set; }

		#endregion

		#region Security Methods
		private void DenyAccess(HttpApplication httpApp)
		{
			httpApp.Response.StatusCode = 401;
			httpApp.Response.StatusDescription = "Access Denied";
			httpApp.Response.Write("401 Access Denied");
			httpApp.CompleteRequest();
		}

		private void context_AuthenticateRequest(object sender, EventArgs e)
		{
			bool _requestAuthorized = true;
			HttpApplication _httpApp = (HttpApplication)sender;


			//Since we are processing all wildcards... 
			//	The web project will not load if we intercept its request.
			//	Therefore... if the User-Agent is the studio... do nothing
			if (_httpApp.Request.Headers["User-Agent"] != null && !_httpApp.Request.Headers["User-Agent"].StartsWith("Microsoft-Visual-Studio.NET"))
			{
				//Check to see if the request needs to be authenticated
				if (this.ModuleAuthentication != Authentication.None)
				{
					AuthenticationArgs _authArgs = new AuthenticationArgs(_httpApp.Request.Url, "", this.ModuleAuthentication);
					AuthorizationArgs _authorizationArgs = new AuthorizationArgs(_authArgs);

					//Fire the event
					this.OnAuthenticateRequest(_authArgs);

					if (_authArgs.ProcessAuthorization)
					{
						_httpApp.Context.Items["WebDAVModule_AuthArgs"] = _authArgs;

						string _authStr = _httpApp.Request.Headers["Authorization"];
						switch (this.ModuleAuthentication)
						{
							case Authentication.Basic:
								//By default the request is not authorized
								_requestAuthorized = false;
								if (!string.IsNullOrEmpty(_authStr) && _authStr.StartsWith("Basic"))
								{
									byte[] _decodedBytes = Convert.FromBase64String(_authStr.Substring(6));
									string[] _authInfo = System.Text.Encoding.ASCII.GetString(_decodedBytes).Split(':');

									BasicAuthorizationArgs _basicAuthArgs = new BasicAuthorizationArgs(_authInfo[0], _authInfo[1], _authArgs.Realm);

									//Set the authorization username
									_authorizationArgs.UserName = _basicAuthArgs.UserName;

									//Fire the event
									this.OnBasicAuthorization(_basicAuthArgs);

									if (_basicAuthArgs.Authorized)
									{
										_requestAuthorized = true;
										_httpApp.Context.User = new GenericPrincipal(new GenericIdentity(_basicAuthArgs.UserName, "Basic"), null);
									}

									_authorizationArgs.RequestAuthorized = _requestAuthorized;

									//Fire the event
									this.OnAuthorizationComplete(_authorizationArgs);
								}
								break;

							case Authentication.Digest:
								//By default the request is not authorized
								_requestAuthorized = false;
								if (!string.IsNullOrEmpty(_authStr) && _authStr.StartsWith("Digest"))
								{
									_authStr = _authStr.Substring(7);

									SortedList<string, string> _authItems = new SortedList<string, string>();
									foreach (string _authItem in _authStr.Split(','))
									{
										string[] _authItemArray = _authItem.Split('=');
										string _authKey = _authItemArray[0].Trim(new char[] { ' ', '\"' });
										string _authValue = _authItemArray[1].Trim(new char[] { ' ', '\"' });

										_authItems[_authKey] = _authValue;
									}

									DigestAuthorizationArgs _digestAuthArgs = new DigestAuthorizationArgs(_authItems["username"], _authItems["realm"]);

									//Set the authorization username
									_authorizationArgs.UserName = _digestAuthArgs.UserName;

									//Fire the event
									this.OnDigestAuthorization(_digestAuthArgs);

									//Validate password
									string _userInfo = String.Format("{0}:{1}:{2}", _authItems["username"], _authArgs.Realm, _digestAuthArgs.Password);
									string _hashedUserInfo = GetMD5HashBinHex(_userInfo);

									string _uriInfo = String.Format("{0}:{1}", _httpApp.Request.HttpMethod, _authItems["uri"]);
									string _hashedUriInfo = GetMD5HashBinHex(_uriInfo);

									string _nonceInfo = null;
									if (_authItems.ContainsKey("qop"))
									{
										_nonceInfo = String.Format
														(
															"{0}:{1}:{2}:{3}:{4}:{5}",
															new object[] { 
																_hashedUserInfo, 
																_authItems["nonce"], 
																_authItems["nc"], 
																_authItems["cnonce"], 
																_authItems["qop"], 
																_hashedUriInfo 
															}
														);
									}
									else
									{
										_nonceInfo = String.Format
														(
															"{0}:{1}:{2}",
															_hashedUserInfo,
															_authItems["nonce"],
															_hashedUriInfo
														);
									}

									string _hashedNonceInfo = GetMD5HashBinHex(_nonceInfo);

									bool _staleNonce = !this.IsValidNonce(_authItems["nonce"]);
									_httpApp.Context.Items["WebDAVModule_DigestStaleNonce"] = _staleNonce;

									if (_authItems["response"] == _hashedNonceInfo && !_staleNonce)
									{
										_requestAuthorized = true;
										_httpApp.Context.User = new GenericPrincipal(new GenericIdentity(_digestAuthArgs.UserName, "Digest"), null);
									}

									_authorizationArgs.RequestAuthorized = _requestAuthorized;

									//Fire the event
									this.OnAuthorizationComplete(_authorizationArgs);
								}
								break;
						}
					}
				}

				if (!_requestAuthorized)
					DenyAccess(_httpApp);
				else
				{
					//Check to see if we should process the request
					DavProcessRequestArgs _processRequestArgs = new DavProcessRequestArgs(_httpApp.Context, this.IsWebDAVRequest);

					//Fire the event
					this.OnProcessRequest(_processRequestArgs);

					if (_processRequestArgs.ProcessRequest)
					{
						this.WebDavProcessor.ProcessRequest(_httpApp);
					}

					//Fire the event
					this.OnRequestProcessed();
				}
			}
		}

		void context_EndRequest(object sender, EventArgs e)
		{
			HttpApplication _httpApp = (HttpApplication)sender;
			if (_httpApp.Response.StatusCode == 401)
			{
				AuthenticationArgs _authArgs = (AuthenticationArgs)_httpApp.Context.Items["WebDAVModule_AuthArgs"];

				if (_authArgs != null)
				{
					switch (this.ModuleAuthentication)
					{
						case Authentication.Basic:
							string _authHeader = String.Format("Basic realm=\"{0}\"", _authArgs.Realm);
							_httpApp.Response.AppendHeader("WWW-Authenticate", _authHeader);
							break;

						case Authentication.Digest:
							bool _isNonceStale = false;
							if (_httpApp.Context.Items["WebDAVModule_DigestStaleNonce"] != null)
								_isNonceStale = (bool)_httpApp.Context.Items["WebDAVModule_DigestStaleNonce"];

							StringBuilder _digestHeader = new StringBuilder("Digest");
							_digestHeader.Append(" realm=\"");
							_digestHeader.Append(_authArgs.Realm);
							_digestHeader.Append("\"");
							_digestHeader.Append(", nonce=\"");
							_digestHeader.Append(this.GetCurrentNonce());
							_digestHeader.Append("\"");
							_digestHeader.Append(", opaque=\"0000000000000000\"");
							_digestHeader.Append(", stale=");
							_digestHeader.Append(!_isNonceStale ? "false" : "true");
							_digestHeader.Append(", algorithm=MD5");
							_digestHeader.Append(", qop=\"auth\"");

							_httpApp.Response.AppendHeader("WWW-Authenticate", _digestHeader.ToString());
							break;
					}
				}
			}
		}

		private string GetCurrentNonce()
		{
			DateTime _nonceTime = DateTime.Now + TimeSpan.FromMinutes(1);
			string _expireStr = _nonceTime.ToString("G");

			byte[] _expireBytes = Encoding.ASCII.GetBytes(_expireStr);
			string _nonce = Convert.ToBase64String(_expireBytes);

			//Trim the = from the end
			_nonce = _nonce.TrimEnd('=');
			return _nonce;
		}

		private string GetMD5HashBinHex(string val)
		{
			byte[] _hash = new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(val));

			StringBuilder _authStr = new StringBuilder();
			for (int i = 0; i < 16; i++)
				_authStr.Append(String.Format("{0:x02}", _hash[i]));

			return _authStr.ToString();
		}

		private bool IsValidNonce(string nonce)
		{
			DateTime _expireTime;
			int _numPadChars = nonce.Length % 4;
			if (_numPadChars > 0)
				_numPadChars = 4 - _numPadChars;

			string _newNonce = nonce.PadRight(nonce.Length + _numPadChars, '=');
			try
			{
				byte[] _decodedBytes = Convert.FromBase64String(_newNonce);
				_expireTime = DateTime.Parse(Encoding.ASCII.GetString(_decodedBytes));
			}
			catch (FormatException)
			{
				return false;
			}

			return _expireTime >= DateTime.Now;
		}
		#endregion

		void context_BeginRequest(object sender, EventArgs e)
		{
			HttpApplication _httpApp = (HttpApplication)sender;

			this.IsWebDAVRequest = false;
			this.RequestHttpMethod = _httpApp.Context.Request.HttpMethod;
			foreach (string _enumName in Enum.GetNames(typeof(DavOptionsBase.HttpMethods)))
			{
				if (this.RequestHttpMethod.Equals(_enumName, StringComparison.InvariantCultureIgnoreCase))
				{
					this.IsWebDAVRequest = true;
					break;
				}
			}
		}

		public void Init(ServiceLocator sl, ref PluginSetting setting)
		{
			EventBroker broker = EventBroker.Instance;
			broker.AuthenticateRequest += context_AuthenticateRequest;
			broker.EndRequest += context_EndRequest;
			broker.BeginRequest += context_BeginRequest;
		}
	}
}
