using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using Kiss.Caching;

namespace Kiss.Web
{
    /// <summary>
    /// 使用<see cref="System.Web.Caching.Cache"/>的缓存Provider
    /// </summary>
    public class WebCacheProvider : ICacheProvider
    {
        private static readonly Cache _cache;

        static WebCacheProvider()
        {
            _cache = HttpRuntime.Cache;
        }

        /// <summary>
        /// 将对象插入缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="validFor"></param>
        public void Insert(string key, object obj, TimeSpan validFor)
        {
            _cache.Insert(key, obj, null, Cache.NoAbsoluteExpiration, validFor);
        }

        /// <summary>
        /// 批量从缓存获取对象
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public IDictionary<string, object> Get(IEnumerable<string> keys)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (string key in keys)
            {
                object o = Get(key);
                if (o != null)
                    dict.Add(key, Get(key));
            }

            return dict;
        }

        /// <summary>
        /// 从缓存移除对象
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        /// <summary>
        /// 从缓存获取对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            return _cache[key];
        }
    }
}
