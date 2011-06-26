using System;

namespace Kiss.Web.WebDAV.Security
{
	/// <summary>
	/// Basic Authorization event arguments
	/// </summary>
	public sealed class BasicAuthorizationArgs : EventArgs
	{
		/// <summary>
		/// Basic Authorization event arguments
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <param name="realm"></param>
		internal BasicAuthorizationArgs(string userName, string password, string realm)
		{
			this.UserName = userName;
			this.Password = password;
			this.Realm = realm;
			this.Authorized = false;
		}

		/// <summary>
		/// User Name
		/// </summary>
		public string UserName { get; private set; }

		/// <summary>
		/// Password
		/// </summary>
		public string Password { get; private set; }

		/// <summary>
		/// Realm
		/// </summary>
		public string Realm { get; private set;}

		/// <summary>
		/// Authorized 
		/// </summary>
		/// <value>
		/// TRUE if the request is authorized
		/// </value>
		public bool Authorized { get; set; }
	}
}
