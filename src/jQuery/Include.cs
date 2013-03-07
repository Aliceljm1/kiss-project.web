using System;
using System.IO;
using System.Web.UI;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    public class Include : Control, IContextAwaredControl
    {
        public string Js { get; set; }
        public string Css { get; set; }
        public bool NoCombine { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (StringUtil.IsNullOrEmpty(Css))
                return;

            ClientScriptProxy proxy = ClientScriptProxy.Current;

            foreach (string css in StringUtil.Split(Css, ",", true, true))
            {
                if (css.Contains("|"))
                {
                    string[] array = StringUtil.Split(css, "|", true, true);
                    if (array.Length != 2) continue;

                    proxy.RegisterCssResource(array[1], array[0]);
                }
                else if (css.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (css.StartsWith("~"))
                        proxy.RegisterCss(ServerUtil.ResolveUrl(css));
                    else
                        proxy.RegisterCss(StringUtil.CombinUrl(CurrentSite.VirtualPath, css));
                }
                else
                    proxy.RegisterCssResource(string.Format("Kiss.Web.jQuery.{0}.css", css));
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            if (StringUtil.IsNullOrEmpty(Js)) return;

            ClientScriptProxy proxy = ClientScriptProxy.Current;

            foreach (string js in StringUtil.Split(Js, ",", true, true))
            {
                if (js.Contains("|"))
                {
                    string[] array = StringUtil.Split(js, "|", true, true);
                    if (array.Length != 2) continue;

                    proxy.RegisterJsResource(writer, array[1], array[0]);
                }
                else if (js.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (js.Contains("*"))
                    {
                        string vp = "/";
                        var index = js.LastIndexOf('/');
                        if (index != -1)
                            vp = js.Substring(0, index + 1);

                        string path;
                        if (vp.StartsWith("."))
                            path = ServerUtil.MapPath(StringUtil.CombinUrl(JContext.Current.ThemePath, vp.Substring(1)));
                        else
                            path = ServerUtil.MapPath(StringUtil.CombinUrl(CurrentSite.VirtualPath, vp));

                        if (!Directory.Exists(Path.GetDirectoryName(path)))
                            continue;

                        foreach (var item in Directory.GetFiles(Path.GetDirectoryName(path), js.Substring(index + 1), SearchOption.AllDirectories))
                        {
                            string relativePath = item.ToLower().Replace(path.ToLower(), string.Empty);
                            relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');

                            if (vp.StartsWith("~"))
                                proxy.RegisterJs(writer, StringUtil.CombinUrl(ServerUtil.ResolveUrl(vp), relativePath), NoCombine);
                            else if (vp.StartsWith("."))
                                proxy.RegisterJs(writer, StringUtil.CombinUrl(JContext.Current.ThemePath, StringUtil.CombinUrl(vp.Substring(1), relativePath)), NoCombine);
                            else
                                proxy.RegisterJs(writer, StringUtil.CombinUrl(CurrentSite.VirtualPath, StringUtil.CombinUrl(vp, relativePath)), NoCombine);
                        }
                    }
                    else
                    {
                        if (js.StartsWith("~"))
                            proxy.RegisterJs(writer, ServerUtil.ResolveUrl(js), NoCombine);
                        else if (js.StartsWith("."))
                            proxy.RegisterJs(writer, StringUtil.CombinUrl(JContext.Current.ThemePath, js.Substring(1)), NoCombine);
                        else
                            proxy.RegisterJs(writer, StringUtil.CombinUrl(CurrentSite.VirtualPath, js), NoCombine);
                    }
                }
                else
                    proxy.RegisterJsResource(writer,
                        GetType(),
                        string.Format("Kiss.Web.jQuery.{0}.js", js), NoCombine);
            }
        }

        private IArea _site;
        public IArea CurrentSite { get { return _site ?? JContext.Current.Site; } set { _site = value; } }
    }
}
