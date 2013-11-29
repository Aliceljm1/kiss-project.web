using Kiss.Caching;
using System;
using System.Collections.Generic;
using System.Web;

namespace Kiss.Web
{
    /// <summary>
    /// use http context items to store datas, datas only survival in current http request
    /// </summary>
    [CacheProvider(Name = "httpcontextcache")]
    public class HttpContextCacheProvider : ICacheProvider
    {
        /// <summary>
        /// batch get 
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public IDictionary<string, object> Get(IEnumerable<string> keys)
        {
            Dictionary<string, object> di = new Dictionary<string, object>();

            if (HttpContext.Current == null) return di;

            foreach (string key in keys)
            {
                object obj = Get(key);
                if (obj != null)
                    di.Add(key, obj);
            }
            return di;
        }

        /// <summary>
        /// get objects from httpcontext
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            HttpContext context = HttpContext.Current;
            if (context == null) return null;

            if (context.Items.Contains(key))
                return context.Items[key];
            return null;
        }

        /// <summary>
        /// save obj to http context
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="validFor">useless</param>
        public void Insert(string key, object obj, TimeSpan validFor)
        {
            if (HttpContext.Current == null) return;

            HttpContext.Current.Items[key] = obj;
        }

        /// <summary>
        /// remove objects from http context
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            if (HttpContext.Current == null) return;

            HttpContext.Current.Items.Remove(key);
        }
    }
}
