using System;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Xml;
using Kiss.Config;
using Kiss.Utils;

namespace Kiss.Web
{
    public class RedirectHandler : IHttpHandler, IRequiresSessionState
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string returnUrl = HttpUtility.UrlDecode(context.Request["ReturnUrl"]);

            string key = string.Empty;

            try
            {
                Uri uri = new Uri(returnUrl);
                key = string.Format("{0}:{1}", uri.Host, uri.Port);
            }
            catch (Exception ex)
            {
                context.Response.Write(ExceptionUtil.WriteException(ex));
                return;
            }

            RedirectConfig config = RedirectConfig.Instance;

            if (HttpContext.Current.Session != null && StringUtil.GetBoolean(context.Session[key], false))
                context.Response.Redirect(returnUrl);
            else
            {
                context.Response.Clear();
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.ContentType = "text/html";

                StringBuilder html = new StringBuilder();
                html.AppendFormat("<html><head><title>{0}</title></head><body style='margin:0;padding:0;'>{1}",
                    config.PageTitle,
                    config.PageHtml);

                html.Append("<iframe frameborder='no' border='0' marginwidth='0' marginheight='0' allowtransparency='yes' style='width:100%; height:100%' id='iframe'></iframe>");
                html.AppendFormat("<script type='text/javascript'>var subWin=window.open('{0}','_blank','height=0,width=0,bottom=0,left=0');",
                    RedirectConfig.Instance.SubmitUrl + "?page=" + HttpUtility.UrlEncode(returnUrl));

                html.Append("if( subWin!=null){var interval = setInterval(function(){redirect();}, " + config.Interval + " );}");
                html.Append("function redirect(){if(subWin==null) return;try{var ev = subWin.event;}catch(e){ subWin.close();subWin=null; clearInterval(interval); document.title='';var iframe = document.getElementById('iframe');");
                html.AppendFormat("iframe.src= '{0}';", returnUrl);
                html.Append("}");
                html.Append("}</script></body></html>");

                if (HttpContext.Current.Session != null)
                    context.Session[key] = true;

                context.Response.Write(html.ToString());
                context.Response.End();
            }
        }

        #endregion
    }

    public class RedirectConfig : ConfigBase
    {
        public static RedirectConfig Instance
        {
            get
            {
                return GetConfig<RedirectConfig>("redirectHandler", true);
            }
        }

        public string SubmitUrl { get; private set; }
        public string PageTitle { get; private set; }
        public string PageHtml { get; private set; }
        public int Interval { get; private set; }

        protected override void LoadValuesFromConfigurationXml(XmlNode node)
        {
            base.LoadValuesFromConfigurationXml(node);

            SubmitUrl = XmlUtil.GetStringAttribute(node, "submitUrl", string.Empty);
            PageTitle = XmlUtil.GetStringAttribute(node, "title", "页面跳转中...");
            PageHtml = node.InnerText;
            Interval = XmlUtil.GetIntAttribute(node, "interval", 100);
        }
    }
}
