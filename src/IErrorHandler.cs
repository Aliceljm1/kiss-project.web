using System;
using System.IO;
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
    public class ErrorHandler : IErrorHandler, IAutoStart
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

            JContext.Current.ViewData["_ex"] = ex;
            JContext.Current.ViewData["_msg"] = ExceptionUtil.WriteException(ex, true);

            HttpContext.Current.Response.StatusCode = 500;

            while (ex != null)
            {
                if (!(ex is KissException) && JContext.Current.GetViewData("_title") == null)
                {
                    JContext.Current.ViewData["_title"] = ex.Message;
                }

                if (ex is HttpException)
                {
                    HttpException httpEx = ex as HttpException;
                    HttpContext.Current.Response.StatusCode = httpEx.GetHttpCode();

                    break;
                }

                ex = ex.InnerException;
            }

            NotifyErrorEventArgs arg = new NotifyErrorEventArgs()
            {
                Exception = ex
            };
            OnNotifyError(arg);

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

            ISite site = null;
            try
            {
                site = JContext.Current.Site;
            }
            catch
            {
            }

            if (site == null || StringUtil.IsNullOrEmpty(site.ErrorPage))
            {
                writer.Write("something ERROR happended! see <a href='{0}' target='_blank'>logs</a> for more detail.", ServerUtil.ResolveUrl("~/kiss/logs/1.aspx?SiteKey=default&sort=-DateCreate"));
            }
            else
            {
                ITemplateEngine te = ServiceLocator.Instance.Resolve<ITemplateEngine>();

                using (StringWriter sw = new StringWriter())
                {
                    te.Process(JContext.Current.ViewData, "errorpage", sw, site.ErrorPage);

                    writer.Write(sw.GetStringBuilder().ToString());
                }
            }

            base.Render(writer);
        }
    }
}
