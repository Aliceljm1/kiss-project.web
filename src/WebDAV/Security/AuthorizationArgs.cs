using System;

namespace Kiss.Web.WebDAV.Security
{
	/// <summary>
	/// Authentication event arguments
	/// </summary>
	public sealed class AuthorizationArgs : EventArgs
	{
		/// <summary>
		/// Authorization event arguments
		/// </summary>
		/// <param name="authArgs"></param>
		internal AuthorizationArgs(AuthenticationArgs authArgs)
		{
			this.AuthenticationInfo = authArgs;
		}

		/// <summary>
		/// Authentication Info
		/// </summary>
		public AuthenticationArgs AuthenticationInfo { get; private set; }

		/// <summary>
		/// UserName
		/// </summary>
		public string UserName { get; internal set; }

		/// <summary>
		/// Request Authorization
		/// </summary>
		public bool RequestAuthorized { get; internal set; }
	}
}
