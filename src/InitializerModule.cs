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

            // start component
            ServiceLocator sl = ServiceLocator.Instance;
            sl.Init(delegate
            {
                sl.AddComponent("Kiss.webcontext", typeof(IWebContext), typeof(WebRequestContext));
                sl.AddComponent("Kiss.typeFinder", typeof(ITypeFinder), typeof(WebAppTypeFinder));

                if (!context.Context.IsCustomErrorEnabled && !context.Context.IsDebuggingEnabled)
                    sl.AddComponent("Kiss.errorhandler", typeof(IErrorHandler), typeof(ErrorHandler));
            });
        }

        /// <summary>
        /// <see cref="IHttpModule.Dispose"/>
        /// </summary>
        public void Dispose()
        {
        }
    }
}
