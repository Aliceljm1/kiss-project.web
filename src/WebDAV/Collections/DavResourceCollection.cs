using System;
using System.Collections.ObjectModel;

using Kiss.Web.WebDAV.BaseClasses;

namespace Kiss.Web.WebDAV.Collections
{
	/// <summary>
	/// WebDav Resource Collection.
	/// </summary>
	[Serializable]
	public class DavResourceCollection : Collection<DavResourceBase>
	{
		/// <summary>
		/// WebDav Lock Property Collection.
		/// </summary>
		public DavResourceCollection() { }
	}
}


