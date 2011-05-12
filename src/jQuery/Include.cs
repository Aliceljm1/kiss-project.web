using System;
using System.IO;
using System.Web.UI;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    public class Include : Control
    {
        public string Js { get; set; }
        public string Css { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (StringUtil.IsNullOrEmpty(Css))
                return;

            JContext jc = JContext.Current;

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
                        proxy.RegisterCss(jc.CombinUrl(css));
                }
                else
                    proxy.RegisterCssResource(string.Format("Kiss.Web.jQuery.{0}.css", css));
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            if (StringUtil.IsNullOrEmpty(Js)) return;

            JContext jc = JContext.Current;

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

                        string path = ServerUtil.MapPath(vp);

                        foreach (var item in Directory.GetFiles(Path.GetDirectoryName(path), js.Substring(index + 1), SearchOption.AllDirectories))
                        {
                            if (vp.StartsWith("~"))
                                proxy.RegisterJs(writer, StringUtil.CombinUrl(ServerUtil.ResolveUrl(vp), Path.GetFileName(item)));
                            else
                                proxy.RegisterJs(writer, jc.CombinUrl(StringUtil.CombinUrl(vp, Path.GetFileName(item))));
                        }
                    }
                    else
                    {
                        if (js.StartsWith("~"))
                            proxy.RegisterJs(writer, ServerUtil.ResolveUrl(js));
                        else
                            proxy.RegisterJs(writer, jc.CombinUrl(js));
                    }
                }
                else
                    proxy.RegisterJsResource(writer,
                        string.Format("Kiss.Web.jQuery.{0}.js", js));
            }
        }
    }
}
