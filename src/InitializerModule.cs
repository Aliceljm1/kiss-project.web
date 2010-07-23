using System;
using System.Web;
using System.Reflection;

namespace Kiss.Web
{
    /// <summary>
    /// A HttpModule that attach some event in http context
    /// </summary>
    public class InitializerModule : IHttpModule
    {
        /// <summary>
        ///  <see cref="IHttpModule.Init"/>
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            EventBroker.Instance.Attach(context);

            // start component
            ServiceLocator sl = ServiceLocator.Instance;
            sl.Init(delegate
            {
                sl.AddComponent("Kiss.webcontext", typeof(IWebContext), typeof(WebRequestContext));
                sl.AddComponent("Kiss.typeFinder", typeof(ITypeFinder), typeof(WebAppTypeFinder));

                if (!context.Context.IsCustomErrorEnabled && !context.Context.IsDebuggingEnabled)
                    sl.AddComponent("Kiss.errorhandler", typeof(IErrorHandler), typeof(ErrorHandler));
            });

            // log system hack
            EventBroker.Instance.BeginRequest += BeginRequest;

            StopAppDomainRestart();
        }

        void BeginRequest(object sender, EventArgs e)
        {
            ISite site = JContext.Current.Site;
            if (site != null)
                HttpContext.Current.Application["SITE_KEY"] = site.SiteKey;
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

        /// <summary>
        /// <see cref="IHttpModule.Dispose"/>
        /// </summary>
        public void Dispose()
        {
        }
    }
}
