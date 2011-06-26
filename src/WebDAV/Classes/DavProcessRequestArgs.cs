using System;
using System.Web;

namespace Kiss.Web.WebDAV.Classes
{
	/// <summary>
	/// WebDav Module Process Request event arguments
	/// </summary>
	public sealed class DavProcessRequestArgs : EventArgs
	{
		/// <summary>
		/// WebDav Process Request event arguments
		/// </summary>
		/// <param name="context"></param>
		/// <param name="processRequest"></param>
		public DavProcessRequestArgs(HttpContext context, bool processRequest)
		{
			this.Context = context;
			this.RequestUri = context.Request.Url;
			this.ProcessRequest = processRequest;
		}

		/// <summary>
		/// Request Context
		/// </summary>
		public HttpContext Context { get; private set; }

		/// <summary>
		/// Request Uri
		/// </summary>
		public Uri RequestUri { get; private set; }

		/// <summary>
		/// Argument Lock Timeout
		/// </summary>
		public bool ProcessRequest { get; set; }
	}
}
