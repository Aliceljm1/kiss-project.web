using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.Caching;
using Kiss.Utils;
using Kiss.Web.Mvc;

namespace Kiss.Web.Resources
{
	/// <summary>
	/// 用于合并js，css输出
	/// </summary>
	[Controller("_resc_")]
	class ResourceCombineController : Controller
	{
		private readonly static TimeSpan CACHE_DURATION = TimeSpan.FromDays(30);

		void proc()
		{
			HttpRequest request = httpContext.Request;
			HttpResponse response = httpContext.Response;

			// Read setName, contentType and version. All are required. They are
			// used as cache key
			string files = request["f"] ?? string.Empty;
			string contentType = request["t"] ?? string.Empty;
			string version = request["v"] ?? string.Empty;

			// If the set has already been cached, write the response directly from
			// cache. Otherwise generate the response and cache it
			if (!WriteFromCache(httpContext, files, version, contentType))
			{
				// Load the files defined in querystring and process each file                       
				string[] fileNames = files.Split(new char[] { ',' },
					StringSplitOptions.RemoveEmptyEntries);

				byte[] buffer;
				using (MemoryStream ms = new MemoryStream())
				{
					foreach (string fileName in fileNames)
					{
						byte[] bytes = GetFileContent(httpContext, fileName.Trim());
						if (bytes == null || bytes.Length == 0)
							continue;

						ms.Write(bytes, 0, bytes.Length);
					}

					if (ms.Length == 0) return;

					buffer = ms.ToArray();
				}

				if (buffer == null) return;

				// Cache the combined response so that it can be directly written
				// in subsequent calls 
				httpContext.Cache.Insert(GetCacheKey(files, version),
					buffer, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);

				logger.Debug("refresh combined url: {0}", request.Url.AbsoluteUri);

				// Generate the response
				WriteBytes(buffer, response, contentType);
			}
		}

		private byte[] GetFileContent(HttpContext context, string virtualPath)
		{
			byte[] content = null;

			try
			{
				JContext jc = JContext.Current;
				ISite site = jc.Site;

				string path = virtualPath;

				if (path.IndexOf("_res.aspx") == -1)
				{
					int index = path.IndexOf(site.VirtualPath);
					if (index != -1)
						path = path.Substring(index + site.VirtualPath.Length);

					if (path.StartsWith("themes", StringComparison.InvariantCultureIgnoreCase))
					{
						path = path.Substring(6);

						path = string.Concat(VirtualPathUtility.ToAbsolute(jc.CombinUrl(site.ThemeRoot)), path);
					}
					else if (index != -1)
					{
						path = StringUtil.CombinUrl(site.VirtualPath, path);
					}

					virtualPath = StringUtil.CombinUrl("/", path);

					string physicalPath = context.Server.MapPath(virtualPath);
					content = File.ReadAllBytes(physicalPath);
				}
				else
				{
					NameValueCollection qs = new NameValueCollection();

					foreach (var item in new Url(virtualPath).GetQueries())
					{
						qs[item.Key] = item.Value;
					}
					content = ResourceController.GetResourceContent(qs);
				}
			}
			catch (Exception ex)
			{
				logger.Error("file: {0} is not found. {1}", virtualPath, ExceptionUtil.WriteException(ex));
			}

			if (content == null)
			{
				logger.Error("file: {0} is not null.", virtualPath);
				return null;
			}

			// remove bom byte
			if (content.Length <= 3 || !(content[0] == 0xEF && content[1] == 0xBB && content[2] == 0xBF))
				return content;

			using (var ms = new MemoryStream())
			{
				ms.Write(content, 3, content.Length - 3);

				return ms.ToArray();
			}
		}

		private bool WriteFromCache(HttpContext context, string setName, string version, string contentType)
		{
			byte[] responseBytes = context.Cache[GetCacheKey(setName, version)] as byte[];

			if (null == responseBytes || 0 == responseBytes.Length)
				return false;

			WriteBytes(responseBytes, context.Response, contentType);
			return true;
		}

		private void WriteBytes(byte[] bytes, HttpResponse response, string contentType)
		{
			if (bytes == null || bytes.Length == 0)
				return;

			ContentType = contentType;

			ServerUtil.AddCache(60 * 24 * 90);

			response.BinaryWrite(bytes);
		}

		private string GetCacheKey(string setName, string version)
		{
			return "rescombiner." + setName + "." + version;
		}
	}
}
