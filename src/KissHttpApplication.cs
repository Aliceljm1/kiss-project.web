using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web;
using Kiss.Utils;

namespace Kiss.Web
{
    /// <summary>
    /// 重载了<see cref="System.Web.HttpApplication"/>
    /// </summary>
    public class KissHttpApplication : System.Web.HttpApplication
    {
        private static bool needSetup = false;

        protected void Application_Start(object sender, EventArgs e)
        {
            StopAppDomainRestart();

            // start component
            ServiceLocator sl = ServiceLocator.Instance;
            sl.Init(delegate
            {
                sl.AddComponent("Kiss.webcontext", typeof(IWebContext), typeof(WebRequestContext));
                sl.AddComponent("Kiss.typeFinder", typeof(ITypeFinder), typeof(WebAppTypeFinder));

                if (!Context.IsCustomErrorEnabled && !Context.IsDebuggingEnabled)
                    sl.AddComponent("Kiss.errorhandler", typeof(IErrorHandler), typeof(ErrorHandler));
            });

            LogManager.GetLogger<KissHttpApplication>().Debug("ALL components initialized.");

            // check if system config is valid
            if (Context != null)
            {
                if (ConfigurationManager.GetSection("kiss") == null && Directory.Exists(ServerUtil.MapPath("~/setup")))
                {
                    needSetup = true;
                    Context.Response.Redirect("~/setup/", true);
                }
            }

            EventBroker.Instance.BeginRequest += onBeginRequest;
        }

        public override void Init()
        {
            base.Init();

            EventBroker.Instance.Attach(this);
        }

        protected void Application_End(object sender, EventArgs e)
        {
            string msg = "Application is ending...";

            string url = Application["SITE_URL"] as string;

            if (url != null)
            {
                msg += string.Format("Request url:{0} to restart.", url);

                try
                {
                    Kiss.Utils.Net.HttpRequest.GetPageText(url);
                }
                catch
                {
                }
            }

            LogManager.GetLogger<KissHttpApplication>().Info(msg);
        }

        private void onBeginRequest(object sender, EventArgs e)
        {
            JContext jc = JContext.Current;
            if (jc == null) return;

            HttpContext context = jc.Context;

            if (needSetup)
            {
                if (!context.Response.IsRequestBeingRedirected && jc.Site.SiteKey != "setup")
                    context.Response.Redirect("~/setup/", true);
            }
            else
            {
                if (jc.Site != null)
                    context.Items["SITE_KEY"] = jc.Site.SiteKey;

                // record site url
                if (Application["SITE_URL"] == null)
                    Application["SITE_URL"] = string.Format("{0}://{1}{2}", context.Request.Url.Scheme, context.Request.Url.Authority, context.Request.ApplicationPath);

                if (!context.Response.IsRequestBeingRedirected)
                    context.Response.AddHeader("X-Powered-By", "TXTEK.COM");
            }
        }

        private static void StopAppDomainRestart()
        {
            /// Disable AppDomain restarts when folder is deleted.
            /// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=240686
            PropertyInfo p = typeof(HttpRuntime).GetProperty("FileChangesMonitor",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            if (p == null)
                return;

            object o = p.GetValue(null, null);
            if (o == null) return;

            FieldInfo f = o.GetType().GetField("_dirMonSubdirs",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);

            if (f == null) return;
            object monitor = f.GetValue(o);

            if (monitor == null) return;
            MethodInfo m = monitor.GetType().GetMethod("StopMonitoring",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (m == null) return;
            m.Invoke(monitor, new object[] { });
        }
    }
}
