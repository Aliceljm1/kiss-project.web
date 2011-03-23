using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using Kiss.Utils;

namespace Kiss.Web
{
    public interface IErrorHandler
    {
        void Notify(Exception ex);
    }

    /// <summary>
    /// Is notified of errors in the web context and tries to do something barely useful about them.
    /// </summary>
    public class ErrorHandler : IErrorHandler
    {
        private static ILogger _logger = null;
        private static ILogger logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = LogManager.GetLogger<ErrorHandler>();
                }
                return _logger;
            }
        }

        public class NotifyErrorEventArgs : EventArgs
        {
            public static readonly new NotifyErrorEventArgs Empty = new NotifyErrorEventArgs();

            public Exception Exception { get; set; }
        }

        public static event EventHandler<NotifyErrorEventArgs> NotifyError;

        protected virtual void OnNotifyError(NotifyErrorEventArgs e)
        {
            EventHandler<NotifyErrorEventArgs> handler = NotifyError;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Notify(Exception ex)
        {
            if (ex is HttpUnhandledException)
                ex = ex.InnerException;

            if (ex == null) return;

            logger.Error(ExceptionUtil.WriteException(ex), ex);

            HttpContext.Current.Response.StatusCode = 404;

            while (ex != null)
            {
                if (ex is HttpException)
                {
                    HttpException httpEx = ex as HttpException;
                    HttpContext.Current.Response.StatusCode = httpEx.GetHttpCode();

                    break;
                }

                ex = ex.InnerException;
            }

            OnNotifyError(new NotifyErrorEventArgs() { Exception = ex });

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Server.Transfer(new ErrorPage(), true);
            HttpContext.Current.Response.End();
        }

        #region IAutoStart Members

        public void Start()
        {
            EventBroker.Instance.Error += Application_Error;
        }

        #endregion

        protected virtual void Application_Error(object sender, EventArgs e)
        {
            HttpApplication application = sender as HttpApplication;
            if (application != null)
            {
                Exception ex = application.Server.GetLastError();
                application.Server.ClearError();
                if (ex != null)
                {
                    try
                    {
                        Notify(ex);
                    }
                    catch
                    {
                        application.Context.Response.End();
                    }
                }
            }
        }
    }

    public class ErrorPage : Page
    {
        protected override void Render(HtmlTextWriter writer)
        {
            HttpContext.Current.Response.Clear();

            string filename = Server.MapPath("~/404.html");

            if (!File.Exists(filename))
            {
                writer.Write("ERROR!");
            }
            else
            {
                string response = File.ReadAllText(filename, Encoding.UTF8);
                if (string.IsNullOrEmpty(response))
                    writer.Write("ERROR!");
                else
                    writer.Write(response);
            }

            base.Render(writer);
        }
    }
}
