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
}
