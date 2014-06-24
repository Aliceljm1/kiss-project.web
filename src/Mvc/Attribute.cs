using System;

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// use this attribute to mark class as controller
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class ControllerAttribute : Attribute
    {
        readonly string urlId;

        public ControllerAttribute(string urlId)
        {
            this.urlId = urlId;
        }

        public string UrlId { get { return urlId; } }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class HttpPostAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class HttpGetAttribute : Attribute
    {
        readonly int cacheMinutes;

        public HttpGetAttribute()
        {
        }

        public HttpGetAttribute(int cacheMinutes)
        {
            this.cacheMinutes = cacheMinutes;
        }

        public int CacheMinutes { get { return cacheMinutes; } }
    }
}
