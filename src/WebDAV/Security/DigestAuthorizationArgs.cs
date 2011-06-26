using System;

namespace Kiss.Web.WebDAV.Security
{
	/// <summary>
	/// Digest Authorization event arguments
	/// </summary>
	public sealed class DigestAuthorizationArgs : EventArgs
	{
		/// <summary>
		/// Digest Authorization event arguments
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="realm"></param>
		public DigestAuthorizationArgs(string userName, string realm)
		{
			this.UserName  = userName;
			this.Realm = realm;
		}

		/// <summary>
		/// User Name
		/// </summary>
		public string UserName { get; private set; }

		/// <summary>
		/// Password
		/// </summary>
		/// <remarks>
		/// Required for digest authorization
		/// </remarks>
		public string Password { get; set; }

		/// <summary>
		/// Nonce
		/// </summary>
		/// <remarks>
		/// Update to apply a custom nonce
		/// </remarks>
		public string Nonce { get; set; }

		/// <summary>
		/// Nonce Is Stale
		/// </summary>
		/// <remarks>
		/// Update to require a new nonce
		/// </remarks>
		public bool StaleNonce { get; set; }

		/// <summary>
		/// Realm
		/// </summary>
		public string Realm { get; private set; }
	}
}
