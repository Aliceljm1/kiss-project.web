using System;

namespace Kiss.Web.WebDAV.Classes
{
	/// <summary>
	/// WebDav Lock Refresh event arguments
	/// </summary>
	public sealed class DavRefreshEventArgs : EventArgs, ICloneable
	{
		/// <summary>
		/// WebDav Lock Refresh event arguments
		/// </summary>
		/// <param name="lockToken"></param>
		/// <param name="lockTimeout"></param>
		internal DavRefreshEventArgs(string lockToken, int lockTimeout)
		{
			this.LockToken = lockToken;
			this.LockTimeout = lockTimeout;
		}

		/// <summary>
		/// Argument Lock Token
		/// </summary>
		public string LockToken { get; private set; }

		/// <summary>
		/// Argument Lock Timeout
		/// </summary>
		public int LockTimeout { get; private set; }

		#region ICloneable Members
		// Explicit interface method impl
		object ICloneable.Clone()
		{
			return this.Clone();
		}

		/// <summary>
		/// DavRefreshEventArgs Clone
		/// </summary>
		/// <remarks>Deep copy</remarks>
		/// <returns></returns>
		public DavRefreshEventArgs Clone()
		{
			// Start with a flat, memberwise copy
			DavRefreshEventArgs _davRefreshEventArgs = (DavRefreshEventArgs)this.MemberwiseClone();
			return _davRefreshEventArgs;
		}
		#endregion
	}
}
