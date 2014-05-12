using System;

namespace Kiss.Web.UrlMapping
{
    /// <summary>
    /// use this attribute to mark controls' action
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class UrlRouteAttribute : Attribute
    {
        readonly string template;
        readonly string href;
        string title;

        public UrlRouteAttribute(string template)
        {
            this.template = template;
        }

        public UrlRouteAttribute(string template, string href)
            : this(template)
        {
            this.href = href;
        }

        public UrlRouteAttribute(string template, string href, string title)
            : this(template, href)
        {
            this.title = title;
        }

        public string Template { get { return template; } }

        public string Href { get { return href; } }

        public string Title { get { return title; } set { title = value; } }
    }
}
