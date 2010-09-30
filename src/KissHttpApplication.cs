using System;
using System.Reflection;
using System.Web;

namespace Kiss.Web
{
    /// <summary>
    /// 重载了<see cref="System.Web.HttpApplication"/>
    /// </summary>
    public class KissHttpApplication : System.Web.HttpApplication
    {
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
        }

        public override void Init()
        {
            base.Init();

            EventBroker.Instance.Attach(this);

            EventBroker.Instance.BeginRequest += onBeginRequest;
        }

        private void onBeginRequest(object sender, EventArgs e)
        {
            JContext jc = JContext.Current;

            HttpContext context = HttpContext.Current;

            if (jc != null && jc.Site != null)
                context.Items["SITE_KEY"] = jc.Site.SiteKey;

            // record site url
            if (context.Application["SITE_URL"] == null)
                context.Application["SITE_URL"] = string.Format("{0}://{1}{2}", context.Request.Url.Scheme, context.Request.Url.Authority, context.Request.ApplicationPath);

            jc.Context.Response.AddHeader("X-Powered-By", "TXTEK.COM");
        }

        protected void Application_End(object sender, EventArgs e)
        {
            string msg = "Application is ending...";

            string url = Application["SITE_URL"] as string;

            if (url != null)
            {
                msg += string.Format("Request url:{0} to restart.", url);

                Kiss.Utils.Net.HttpRequest.GetPageText(url);
            }

            LogManager.GetLogger<KissHttpApplication>().Info(msg);
        }

        private static void StopAppDomainRestart()
        {
            /// Disable AppDomain restarts when folder is deleted.
            /// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=240686
            PropertyInfo p = typeof(HttpRuntime).GetProperty("FileChangesMonitor",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            object o = p.GetValue(null, null);

            FieldInfo f = o.GetType().GetField("_dirMonSubdirs",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);

            object monitor = f.GetValue(o);

            MethodInfo m = monitor.GetType().GetMethod("StopMonitoring",
                BindingFlags.Instance | BindingFlags.NonPublic);

            m.Invoke(monitor, new object[] { });
        }
    }
}
