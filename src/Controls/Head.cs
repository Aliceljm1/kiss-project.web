using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.UI;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// 该控件替代了html head, 提供了添加标题, MetaTags, css, opensearch的方法 
    /// </summary>
    [PersistChildren(true), ParseChildren(false)]
    public class Head : Control
    {
        #region const

        private const string metaFormat = "<meta name=\"{0}\" content=\"{1}\" />";
        private const string metaKey = "Kiss.Web.Controls.MetaTags";
        private const string autoDiscoveryKey = "Kiss.Web.Controls.AutoDiscovery";
        private const string styleKey = "Kiss.Web.Controls.StyleTags";
        private const string styleFormat = "<link rel=\"stylesheet\" href=\"{0}\" type=\"text/css\" media=\"{1}\" />";
        private const string autoDiscoveryLink = "<link rel=\"alternate\" type=\"application/{0}+xml\" title=\"{1}\" href=\"{2}\"  />";
        private const string linkFormat = "<link rel=\"{0}\" href=\"{1}\" />";
        private const string linkKey = "Kiss.Web.Controls.LinkTags";
        private const string rawContentKey = "Kiss.Web.Controls.RawHeaderContent";

        #endregion

        private ISite site;

        /// <summary>
        /// page title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// set to ture to use custom settings
        /// </summary>
        public bool UseCustomSettings { get; set; }

        #region override

        protected override void Render(HtmlTextWriter writer)
        {
            site = JContext.Current.Site;

            writer.WriteLine("<head>");
            if (!UseCustomSettings)
                writer.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" />");

            base.Render(writer);

            RenderTitle(site, writer);

            RenderMetaTags(writer);
            RenderLinkTags(writer);

            RenderStyleSheets(writer);
            RenderFavicon(writer);

            RenderAutoDiscoverySyndication(writer);

            RenderAdditionHeader(writer);
            RenderRawContent(writer);

            writer.Write("</head>");
        }

        #endregion

        #region Render Methods

        protected virtual void RenderAdditionHeader(HtmlTextWriter writer)
        {
            if (!string.IsNullOrEmpty(site.RawAdditionalHeader))
            {
                writer.WriteLine(site.RawAdditionalHeader);
            }
        }

        protected virtual void RenderAutoDiscoverySyndication(HtmlTextWriter writer)
        {
            ArrayList autoDiscovery = Context.Items[autoDiscoveryKey] as ArrayList;
            if (autoDiscovery != null && autoDiscovery.Count > 0)
            {
                foreach (string item in autoDiscovery)
                {
                    writer.WriteLine(item);
                }
            }
        }

        protected virtual void RenderStyleSheets(HtmlTextWriter writer)
        {
            RenderStyleSheets(writer, StyleRelativePosition.First);
            RenderStyleSheets(writer, StyleRelativePosition.Unspecified);
            RenderStyleSheets(writer, StyleRelativePosition.Last);
        }

        protected virtual void RenderStyleSheets(HtmlTextWriter writer, StyleRelativePosition position)
        {
            Queue queue = Context.Items[styleKey] as Queue;
            if (queue == null || queue.Count == 0)
                return;

            Dictionary<string, List<string>> di = new Dictionary<string, List<string>>();
            List<string> otherurls = new List<string>();

            foreach (StyleQueueItem si in queue)
            {
                if (si.Position != position) continue;

                if (!site.CombinCss || !si.ForceCombin)
                    writer.WriteLine(si.StyleTag);
                else if (site.CombinCss)
                {
                    string url = si.Url;
                    int index = url.LastIndexOf("/");
                    if (index == -1)
                    {
                        otherurls.Add(url);
                        continue;
                    }

                    string path = url.Substring(0, index + 1);
                    if (!di.ContainsKey(path))
                        di[path] = new List<string>();

                    di[path].Add(url);
                }
            }

            if (site.CombinCss)
            {
                foreach (string path in di.Keys)
                {
                    string dir = path;
                    int index = path.IndexOf(site.VirtualPath);
                    if (index != -1)
                        dir = path.Substring(index + site.VirtualPath.Length);
                    writer.WriteLine(string.Format(styleFormat,
                                        Utility.FormatCssUrl(string.Format("{2}rescombiner.axd?f={0}&t=text/css&v={1}",
                                                                ServerUtil.UrlEncode(StringUtil.CollectionToCommaDelimitedString(di[path])),
                                                                site.CssVersion,
                                                                dir)),
                                        "screen"));
                }
            }
        }

        protected virtual void RenderMetaTags(HtmlTextWriter writer)
        {
            NameValueCollection metaTags = Context.Items[metaKey] as NameValueCollection;
            if (metaTags == null)
                metaTags = new NameValueCollection();

            // todo : move to page
            //string keyword = metaTags[ "keywords" ] ?? site.SearchMetaKeywords;
            //if( StringUtil.HasText( keyword ) )
            //    metaTags[ "keywords" ] = site.SearchMetaKeywords;

            //string desc = ( metaTags[ "description" ] ?? JContext.Current.Navigation.Desc ) ?? site.SearchMetaDescription;
            //if( StringUtil.HasText( desc ) )
            //    metaTags[ "description" ] = desc;

            //metaTags["GENERATOR"] = site.Generator ?? "KISS Project";

            foreach (string key in metaTags.Keys)
            {
                writer.WriteLine(metaFormat, key, metaTags[key]);
            }
        }

        protected virtual void RenderLinkTags(HtmlTextWriter writer)
        {
            NameValueCollection linkTags = Context.Items[linkKey] as NameValueCollection;
            if (linkTags == null)
                linkTags = new NameValueCollection();

            foreach (string key in linkTags.Keys)
            {
                writer.WriteLine(linkFormat, key, linkTags[key]);
            }
        }

        protected virtual void RenderTitle(ISite site, HtmlTextWriter writer)
        {
            string title = Title ?? "$!jc.navigation.Title - $!jc.site.title";

            if (!string.IsNullOrEmpty(title) && title.Contains("$"))
            {
                ITemplateEngine te = ServiceLocator.Instance.Resolve<ITemplateEngine>();
                using (StringWriter sw = new StringWriter())
                {
                    te.Process(JContext.Current.ViewData, "title", sw, title);
                    title = sw.GetStringBuilder().ToString();
                }
            }

            if (StringUtil.HasText(title))
                writer.WriteLine("<title>{0}</title>", title);
        }

        protected virtual void RenderRawContent(HtmlTextWriter writer)
        {
            ArrayList rawContent = Context.Items[rawContentKey] as ArrayList;
            if (rawContent == null)
                rawContent = new ArrayList();

            foreach (string item in rawContent)
            {
                writer.WriteLine(item);
            }
        }

        protected virtual void RenderFavicon(HtmlTextWriter writer)
        {
            if (StringUtil.HasText(site.FavIcon))
                writer.WriteLine("<link rel=\"shortcut icon\" type=\"image/ico\" href=\"{0}\" />", site.FavIcon);
        }

        #endregion

        #region MetaTags
        /// <summary>
        /// Adds the description meta tag.
        /// </summary>
        public static void AddMetaDescription(string value, HttpContext context)
        {
            AddMetaTag("description", value, context);
        }

        /// <summary>
        /// Adds the keywork meta tag
        /// </summary>
        public static void AddMetaKeywords(string value, HttpContext context)
        {
            AddMetaTag("keywords", value, context);
        }

        /// <summary>
        /// Adds a new meta tag key and value
        /// </summary>
        public static void AddMetaTag(string key, string value, HttpContext context)
        {
            if (!StringUtil.IsNullOrEmpty(key) && !StringUtil.IsNullOrEmpty(value))
            {
                NameValueCollection mc = context.Items[metaKey] as NameValueCollection;
                if (mc == null)
                {
                    mc = new NameValueCollection();
                    context.Items.Add(metaKey, mc);
                }
                mc[key] = value;
            }
        }
        #endregion

        #region Title

        public static void AddTitle(string title, HttpContext context)
        {
            JContext.Current.Navigation.Title = title;
        }

        #endregion

        #region Style

        public static void AddStyle(string url)
        {
            AddStyle(url, "screen", HttpContext.Current, true);
        }

        /// <summary>
        /// Adds a style to a Queue collection. Style items are always rendered first in first out
        /// Although some some control can be offered using StyleRelativePosition
        /// </summary>
        public static void AddStyle(string url, string media, HttpContext context, bool enqueue)
        {
            AddStyle(url, media, context, StyleRelativePosition.Unspecified, enqueue);
        }

        public static void AddStyle(string url, string media, HttpContext context, StyleRelativePosition position, bool enqueue)
        {
            Queue styleQueue = context.Items[styleKey] as Queue;
            if (styleQueue == null)
            {
                styleQueue = new Queue();
                context.Items[styleKey] = styleQueue;
            }
            styleQueue.Enqueue(new StyleQueueItem(string.Format(styleFormat, url, media), position, url, enqueue));
        }

        #endregion

        #region Syndication AutoDiscovery

        /// <summary>
        /// Adds a RSS 2.0 autodiscoverable link to the header
        /// </summary>
        public static void AddRssAutoDiscovery(string title, string url, HttpContext context)
        {
            SetAutoDiscoverty(title + " (RSS 2.0)", url, context, "rss");
        }

        /// <summary>
        /// Adds an Atom 1.0 autodiscoverale link to the header
        /// </summary>
        public static void AddAtomDiscovery(string title, string url, HttpContext context)
        {
            SetAutoDiscoverty(title + " (Atom 1.0)", url, context, "atom");
        }

        private static void SetAutoDiscoverty(string title, string url, HttpContext context, string autoType)
        {
            ArrayList mc = context.Items[autoDiscoveryKey] as ArrayList;
            if (mc == null)
            {
                mc = new ArrayList();
                context.Items.Add(autoDiscoveryKey, mc);
            }

            if (!mc.Contains(string.Format(autoDiscoveryLink, autoType, title, url)))
                mc.Add(string.Format(autoDiscoveryLink, autoType, title, url));
        }

        #endregion

        #region Link Tags
        /// <summary>
        /// Adds a new link tag rel and href
        /// </summary>
        public static void AddLinkTag(string rel, string href, HttpContext context)
        {
            if (!StringUtil.IsNullOrEmpty(rel) && !StringUtil.IsNullOrEmpty(href))
            {
                NameValueCollection lc = context.Items[linkKey] as NameValueCollection;
                if (lc == null)
                {
                    lc = new NameValueCollection();
                    context.Items.Add(linkKey, lc);
                }
                lc[rel] = href;
            }
        }
        #endregion

        #region Raw Content

        /// <summary>
        /// Adds raw content to the HTML Header, such as script blocks or custom tags
        /// </summary>
        public static void AddRawContent(string content, HttpContext context)
        {
            if (!StringUtil.IsNullOrEmpty(content))
            {
                ArrayList mc = context.Items[rawContentKey] as ArrayList;
                if (mc == null)
                {
                    mc = new ArrayList();
                    context.Items.Add(rawContentKey, mc);
                }
                mc.Add(content);
            }
        }

        #endregion
    }
}
