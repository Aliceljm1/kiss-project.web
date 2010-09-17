using System;
using System.Web;

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

            // log system hack
            EventBroker.Instance.BeginRequest += BeginRequest;
        }

        void BeginRequest(object sender, EventArgs e)
        {
            JContext jc = JContext.Current;

            if (jc != null && jc.Site != null)
                HttpContext.Current.Application["SITE_KEY"] = jc.Site.SiteKey;

            jc.Context.Response.AddHeader("X-Powered-By", "TXTEK.com");
        }        

        /// <summary>
        /// <see cref="IHttpModule.Dispose"/>
        /// </summary>
        public void Dispose()
        {
        }
    }
}
