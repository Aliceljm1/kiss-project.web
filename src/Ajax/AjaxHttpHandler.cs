using System;
using System.Web;
using System.Web.SessionState;
using Kiss.Utils;
using Kiss.Web.UrlMapping;
using Kiss.Web.Utils;

namespace Kiss.Web.Ajax
{
    public class AjaxHttpHandler : IHttpHandler, IReadOnlySessionState
    {
        #region fields

        public const string CLASS_ID_PARAM = "classId";
        public const string METHOD_NAME_PARAM = "methodName";
        public const string METHOD_ARGS_PARAM = "methodArgs";
        public const string AJAX_EXCEPTION_UNID = "__AjaxException";
        public const string QUERYSTRING = "querystring";
        public const string JSONP = "jsonp";

        #endregion

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            JContext jc = JContext.Current;
            // set a ajax request token
            jc.IsAjaxRequest = true;

            // get querystring
            string qs = context.Request.Params[QUERYSTRING];
            if (StringUtil.HasText(qs))
            {
                qs = qs.TrimStart('?');

                jc.QueryString.Add(StringUtil.DelimitedEquation2NVCollection("&", qs));
            }

            if (context.Request.UrlReferrer != null)
            {
                UrlMappingModule module = UrlMappingModule.Instance;
                if (module != null)
                    jc.QueryString.Add(module.GetMappedQueryString(context.Request.UrlReferrer.AbsolutePath));
            }

            // set view data 
            UrlMappingModule.SetViewData();

            string classId = context.Request.Params[CLASS_ID_PARAM];
            string methodName = context.Request.Params[METHOD_NAME_PARAM];
            string methodJsonArgs = context.Request.Params[METHOD_ARGS_PARAM];
            string jsonp = context.Request.Params[JSONP];

            object result;

            int cacheMinutes = -1;
            if (string.IsNullOrEmpty(classId) || string.IsNullOrEmpty(methodName))
            {
                result = "null";
            }
            else
            {
                AjaxConfiguration config = AjaxConfiguration.GetConfig();

                AjaxMethod m = null;

                try
                {
                    AjaxClass c = config.FindClass(classId);

                    m = config.FindMethod(c, methodName);

                    if (string.Equals("Post", m.AjaxType, StringComparison.InvariantCultureIgnoreCase))
                        cacheMinutes = -1;
                    else if (StringUtil.HasText(m.CacheTest))
                        cacheMinutes = methodJsonArgs.Equals(m.CacheTest) ? cacheMinutes : -1;

                    if (c.Type != null)
                        result = m.Invoke(c.Type, methodJsonArgs);
                    else
                        result = m.Invoke(c.TypeString, methodJsonArgs);
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger<AjaxHttpHandler>().Error("ajax handler error." + ExceptionUtil.WriteException(ex));

                    AjaxServerException ajaxEx = null;
                    if (m != null)
                        ajaxEx = m.Exception;

                    if (ajaxEx != null)
                        result = new Kiss.Json.JavaScriptSerializer().DeserializeObject("{ \"" + AJAX_EXCEPTION_UNID + "\": { \"action\":\"" + ajaxEx.Action + "\", \"parameter\":\"" + ajaxEx.Parameter + "\" }}");
                    else
                        result = null;
                }

                ResponseUtil.OutputJson(context.Response, result, cacheMinutes, jsonp);
            }
        }

        #endregion
    }
}
