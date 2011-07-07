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
                {
                    ErrorHandler eh = new ErrorHandler();
                    eh.Start();
                    sl.AddComponentInstance<IErrorHandler>(eh);
                }
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
