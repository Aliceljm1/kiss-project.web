namespace Kiss.Web.WebDAV.Interfaces
{
	/// <summary>
	/// Summary description for IDavRequest.
	/// </summary>
	internal interface ICacheableDavResponse
	{
		/// <summary>
		/// Cache TTL in seconds
		/// </summary>
		int CacheTTL {get; }

		/// <summary>
		/// Cache Key
		/// </summary>
		string CacheKey { get; }
	}
}