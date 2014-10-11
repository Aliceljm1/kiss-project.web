using Kiss.Utils;
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
            });

            LogManager.GetLogger<KissHttpApplication>().Debug("ALL components initialized.");

            EventBroker.Instance.BeginRequest += onBeginRequest;
        }

        public override void Init()
        {
            base.Init();

            EventBroker.Instance.Attach(this);
        }

        private void onBeginRequest(object sender, EventArgs e)
        {
            if (EventBroker.IsStaticResource((sender as HttpApplication).Request))
                return;

            JContext jc = JContext.Current;
            if (jc == null) return;

            HttpContext context = jc.Context;

            if (context.Request.Url.AbsolutePath.IndexOf("_res.aspx", StringComparison.InvariantCultureIgnoreCase) != -1
                || context.Request.Url.AbsolutePath.IndexOf("_resc.aspx", StringComparison.InvariantCultureIgnoreCase) != -1)
                return;

            if (jc.Area != null)
                context.Items["SITE_KEY"] = jc.Area.AreaKey;

            if (!context.Response.IsRequestBeingRedirected)
                context.Response.AddHeader("X-Powered-By", "TXTEK");

            context.Items["_PAGE_ID_"] = StringUtil.UniqueId();
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
