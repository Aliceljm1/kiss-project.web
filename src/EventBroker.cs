using System;
using System.Threading;
using System.Web;

namespace Kiss.Web
{
    /// <summary>
    /// A broker for events from the http application. The purpose of this 
    /// class is to reduce the risk of temporary errors during initialization
    /// causing the site to be crippled.
    /// </summary>
    public class EventBroker
    {
        static EventBroker()
        {
            Instance = new EventBroker();
        }

        /// <summary>Accesses the event broker singleton instance.</summary>
        public static EventBroker Instance
        {
            get { return Singleton<EventBroker>.Instance; }
            protected set { Singleton<EventBroker>.Instance = value; }
        }

        private int observedApplications = 0;

        /// <summary>Attaches to events from the application instance.</summary>
        public virtual void Attach(HttpApplication app)
        {
            Interlocked.Increment(ref observedApplications);

            LogManager.GetLogger<EventBroker>().Info("Attach to HttpApplication {0}.", observedApplications);

            app.BeginRequest += app_BeginRequest;
            app.AuthenticateRequest += app_AuthenticateRequest;
            app.AuthorizeRequest += app_AuthorizeRequest;
            app.PostMapRequestHandler += app_PostMapRequestHandler;
            app.AcquireRequestState += app_AcquireRequestState;
            app.PostAcquireRequestState += app_PostAcquireRequestStateHandler;
            app.PreRequestHandlerExecute += app_PreRequestHandlerExecute;
            app.Error += app_Error;

            app.ReleaseRequestState += app_ReleaseRequestState;
            app.PreSendRequestHeaders += app_PreSendRequestHeaders;

            app.EndRequest += app_EndRequest;

            app.Disposed += app_Disposed;
        }

        /// <summary>Detaches events from the application instance.</summary>
        void app_Disposed(object sender, EventArgs e)
        {
            Interlocked.Decrement(ref observedApplications);

            LogManager.GetLogger<EventBroker>().Info("Detach to HttpApplication {0}.", observedApplications);

            //HttpApplication app = sender as HttpApplication;

            //app.BeginRequest -= app_BeginRequest;
            //app.AuthenticateRequest -= app_AuthenticateRequest;
            //app.AuthorizeRequest -= app_AuthorizeRequest;
            //app.AcquireRequestState -= app_AcquireRequestState;
            //app.PostMapRequestHandler -= app_PostMapRequestHandler;
            //app.Error -= app_Error;

            //app.ReleaseRequestState -= app_ReleaseRequestState;
            //app.PreSendRequestHeaders -= app_PreSendRequestHeaders;

            //app.EndRequest -= app_EndRequest;
        }

        public EventHandler<EventArgs> BeginRequest;
        public EventHandler<EventArgs> AuthenticateRequest;
        public EventHandler<EventArgs> AuthorizeRequest;
        public EventHandler<EventArgs> PostMapRequestHandler;
        public EventHandler<EventArgs> AcquireRequestState;
        public EventHandler<EventArgs> PostAcquireRequestState;
        public EventHandler<EventArgs> PreRequestHandlerExecute;
        public EventHandler<EventArgs> Error;
        public EventHandler<EventArgs> EndRequest;
        public EventHandler<EventArgs> ReleaseRequestState;
        public EventHandler<EventArgs> PreSendRequestHeaders;

        protected void app_BeginRequest(object sender, EventArgs e)
        {
            if (BeginRequest != null)
                BeginRequest(sender, e);
        }

        void app_AuthenticateRequest(object sender, EventArgs e)
        {
            if (AuthenticateRequest != null)
                AuthenticateRequest(sender, e);
        }


        protected void app_AuthorizeRequest(object sender, EventArgs e)
        {
            if (AuthorizeRequest != null)
                AuthorizeRequest(sender, e);
        }

        void app_PostMapRequestHandler(object sender, EventArgs e)
        {
            if (PostMapRequestHandler != null)
                PostMapRequestHandler(sender, e);
        }

        protected void app_AcquireRequestState(object sender, EventArgs e)
        {
            if (AcquireRequestState != null)
                AcquireRequestState(sender, e);
        }

        void app_PostAcquireRequestStateHandler(object sender, EventArgs e)
        {
            if (PostAcquireRequestState != null)
                PostAcquireRequestState(sender, e);
        }

        void app_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (PreRequestHandlerExecute != null)
                PreRequestHandlerExecute(sender, e);
        }

        protected void app_Error(object sender, EventArgs e)
        {
            if (Error != null)
                Error(sender, e);
        }

        protected void app_ReleaseRequestState(object sender, EventArgs e)
        {
            if (ReleaseRequestState != null)
                ReleaseRequestState(sender, e);
        }

        protected void app_PreSendRequestHeaders(object sender, EventArgs e)
        {
            if (PreSendRequestHeaders != null)
                PreSendRequestHeaders(sender, e);
        }

        protected void app_EndRequest(object sender, EventArgs e)
        {
            if (EndRequest != null)
                EndRequest(sender, e);
        }

        /// <summary>
        /// Returns true if the requested resource is one of the typical resources that needn't be processed.
        /// </summary>
        /// <param name="sender">The event sender, probably a http application.</param>
        /// <returns>True if the request targets a static resource file.</returns>
        /// <remarks>
        /// These are the file extensions considered to be static resources:
        /// .css
        ///	.gif
        /// .png 
        /// .jpg
        /// .jpeg
        /// .js
        /// </remarks>
        public static bool IsStaticResource(HttpRequest request)
        {
            if (request != null)
            {
                string extension = VirtualPathUtility.GetExtension(request.Path);

                if (extension == null) return false;

                switch (extension.ToLower())
                {
                    case ".css":
                    case ".gif":
                    case ".bmp":
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".js":
                    case ".ico":
                    case ".swf":
                        return true;
                }
            }

            return false;
        }
    }
}
