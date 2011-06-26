using System.IO;
using Kiss.Web.WebDAV.BaseClasses;

namespace Kiss.Web.WebDAV.Resources
{
	/// <summary>
	/// Dav File Resource Framework Base Class
	/// </summary>
	public class DavFile : DavResourceBase
	{
		/// <summary>
		/// Initializes a new instance of the DavFile class
		/// </summary>
		/// <param name="displayName">Resource display name</param>
		/// <param name="filePath">Resource path</param>
		public DavFile(string displayName, string filePath)
			: base(displayName, filePath)
		{
		}

		/// <summary>
		/// Dav File Content Type
		/// </summary>
		public override string ContentType
		{
			get
			{
				return InternalFunctions.GetMimeType(Path.GetExtension(this.FilePath));
			}
		}

		/// <summary>
		/// Dav File Resource Type
		/// </summary>
		public override ResourceType ResourceType
		{
			get
			{
				return ResourceType.Resource;
			}
		}

		/// <summary>
		/// Gets the DavFile Path
		/// </summary>
		[ResourcePropertyAttribute(false)]
		public string FilePath
		{
			get
			{
				return this.ResourcePath;
			}
		}
	}
}
