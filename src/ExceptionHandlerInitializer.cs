using Kiss.Plugin;
using Kiss.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace Kiss.Web
{
    [AutoInit(Title = "异常处理")]
    class ExceptionHandlerInitializer : IPluginInitializer
    {
        public void Init(ServiceLocator sl, ref PluginSetting setting)
        {
            if (string.IsNullOrEmpty(setting["enable"]))
                setting.Enable = !HttpContext.Current.IsDebuggingEnabled;

            if (!setting.Enable) return;

            ErrorHandler eh = new ErrorHandler();
            eh.Start();
            sl.AddComponentInstance<IErrorHandler>(eh);

            List<string> emails = new List<string>(StringUtil.Split(setting["notifyEmails"], StringUtil.Comma, true, true));

            if (emails.Count == 0) return;

            ErrorHandler.NotifyError += (sender, e) =>
            {
                try
                {
                    Dictionary<string, object> param = new Dictionary<string, object>();
                    param["jc"] = JContext.Current;
                    param["exception"] = ExceptionUtil.WriteException(e.Exception);

                    Kiss.Notice.NoticeFactory.Create("Email").Send("未处理异常", param,
                        null,
                        emails.ToArray());
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger<ExceptionHandlerInitializer>().Error(ExceptionUtil.WriteException(ex));
                }
            };
        }
    }

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

            logger.Error(HttpContext.Current.Request.RawUrl + ExceptionUtil.WriteException(ex));

            OnNotifyError(new NotifyErrorEventArgs() { Exception = ex });

            int statusCode = HttpContext.Current.Response.StatusCode;

            while (ex != null)
            {
                if (ex is HttpException)
                {
                    statusCode = (ex as HttpException).GetHttpCode();

                    break;
                }

                ex = ex.InnerException;
            }

            HttpContext.Current.Response.Clear();

            JContext jc = JContext.Current;

            HttpContext.Current.Response.StatusCode = 200;
            HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;

            if (!File.Exists(HttpContext.Current.Server.MapPath("~/error.htm")))
            {
                logger.Warn("未找到error.htm文件。");

                if (jc.IsAjaxRequest)
                {
                    HttpContext.Current.Response.Write(new Kiss.Json.JavaScriptSerializer().Serialize(new AjaxServerException()
                    {
                        Action = AjaxServerExceptionAction.JSEval,
                        Parameter = "alert('您的访问出错了')"
                    }.ToJson()));
                }
                else
                {
                    HttpContext.Current.Response.Write("<h1>:(</h1>您的访问出错了");
                }
            }
            else
            {
                string error_pageUrl = jc.url("~error.htm?code=" + statusCode);

                if (jc.IsAjaxRequest)
                {
                    HttpContext.Current.Response.Write(new Kiss.Json.JavaScriptSerializer().Serialize(new AjaxServerException()
                    {
                        Action = AjaxServerExceptionAction.JSEval,
                        Parameter = string.Format("window.location='{0}'", error_pageUrl)
                    }.ToJson()));
                }
                else
                {
                    HttpContext.Current.Response.Redirect(error_pageUrl);
                }
            }

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
}
