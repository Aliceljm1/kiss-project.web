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
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

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
