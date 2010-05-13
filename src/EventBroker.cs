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

        private int observedApplications;

        /// <summary>Attaches to events from the application instance.</summary>
        public virtual void Attach(HttpApplication app)
        {
            Interlocked.Increment(ref observedApplications);

            app.BeginRequest += Application_BeginRequest;
            app.AuthenticateRequest += application_AuthenticateRequest;
            app.AuthorizeRequest += Application_AuthorizeRequest;
            app.AcquireRequestState += Application_AcquireRequestState;
            app.PostMapRequestHandler += application_PostMapRequestHandler;
            app.Error += Application_Error;

            app.ReleaseRequestState += Application_ReleaseRequestState;
            app.PreSendRequestHeaders += Application_PreSendRequestHeaders;

            app.EndRequest += Application_EndRequest;

            app.Disposed += Application_Disposed;
        }

        /// <summary>Detaches events from the application instance.</summary>
        void Application_Disposed(object sender, EventArgs e)
        {
            Interlocked.Decrement(ref observedApplications);

            HttpApplication app = sender as HttpApplication;

            app.BeginRequest -= Application_BeginRequest;
            app.AuthenticateRequest -= application_AuthenticateRequest;
            app.AuthorizeRequest -= Application_AuthorizeRequest;
            app.AcquireRequestState -= Application_AcquireRequestState;
            app.PostMapRequestHandler -= application_PostMapRequestHandler;
            app.Error -= Application_Error;

            app.ReleaseRequestState -= Application_ReleaseRequestState;
            app.PreSendRequestHeaders -= Application_PreSendRequestHeaders;

            app.EndRequest -= Application_EndRequest;
        }

        public EventHandler<EventArgs> BeginRequest;
        public EventHandler<EventArgs> AuthenticateRequest;
        public EventHandler<EventArgs> AuthorizeRequest;
        public EventHandler<EventArgs> AcquireRequestState;
        public EventHandler<EventArgs> PostMapRequestHandler;
        public EventHandler<EventArgs> Error;
        public EventHandler<EventArgs> EndRequest;
        public EventHandler<EventArgs> ReleaseRequestState;
        public EventHandler<EventArgs> PreSendRequestHeaders;

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (BeginRequest != null && !IsStaticResource(sender))
                BeginRequest(sender, e);
        }

        void application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (AuthenticateRequest != null && !IsStaticResource(sender))
                AuthenticateRequest(sender, e);
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            if (AcquireRequestState != null && !IsStaticResource(sender))
                AcquireRequestState(sender, e);
        }

        protected void Application_AuthorizeRequest(object sender, EventArgs e)
        {
            if (AuthorizeRequest != null && !IsStaticResource(sender))
                AuthorizeRequest(sender, e);
        }

        void application_PostMapRequestHandler(object sender, EventArgs e)
        {
            if (PostMapRequestHandler != null && !IsStaticResource(sender))
                PostMapRequestHandler(sender, e);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            if (Error != null && !IsStaticResource(sender))
                Error(sender, e);
        }

        protected void Application_ReleaseRequestState(object sender, EventArgs e)
        {
            if (ReleaseRequestState != null && !IsStaticResource(sender))
                ReleaseRequestState(sender, e);
        }

        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            if (PreSendRequestHeaders != null && !IsStaticResource(sender))
                PreSendRequestHeaders(sender, e);
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            if (EndRequest != null && !IsStaticResource(sender))
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
        /// .axd
        /// .ashx
        /// </remarks>
        protected static bool IsStaticResource(object sender)
        {
            HttpApplication application = sender as HttpApplication;
            if (application != null)
            {
                return IsStaticResource(application.Request);
            }
            return false;
        }

        protected static bool IsStaticResource(HttpRequest request)
        {
            if (request != null)
            {
                string extension = VirtualPathUtility.GetExtension(request.Path);

                if (extension == null) return false;

                switch (extension.ToLower())
                {
                    case ".css":
                    case ".gif":
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".js":
                    case ".axd":
                    case ".ashx":
                    case ".ico":
                    case ".swf":
                        return true;
                }
            }

            return false;
        }
    }
}
