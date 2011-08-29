using System;
using System.Web;
using Kiss.Utils;
using Kiss.Web.Mvc;
using Kiss.Web.UrlMapping;
using Kiss.Web.Utils;

namespace Kiss.Web.Ajax
{
    [Controller("_ajax_")]
    public sealed class AjaxController : Controller
    {
        #region fields

        public const string CLASS_ID_PARAM = "classId";
        public const string METHOD_NAME_PARAM = "methodName";
        public const string METHOD_ARGS_PARAM = "methodArgs";
        public const string JSONP = "jsonp";

        #endregion

        #region events

        public class BeforeExecuteEventArgs : BeforeActionExecuteEventArgs
        {
            public static readonly new BeforeExecuteEventArgs Empty = new BeforeExecuteEventArgs();

            public string TypeName { get; set; }
            public string MethodName { get; set; }
        }

        public static event EventHandler<BeforeExecuteEventArgs> BeforeExecute;

        public void OnBeforeExecute(BeforeExecuteEventArgs e)
        {
            EventHandler<BeforeExecuteEventArgs> handler = BeforeExecute;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public static event EventHandler<AfterActionExecuteEventArgs> AfterExecute;

        public void OnAfterExecute(object result)
        {
            EventHandler<AfterActionExecuteEventArgs> handler = AfterExecute;

            if (handler != null)
            {
                handler(this, new AfterActionExecuteEventArgs() { JContext = jc, Result = result });
            }
        }

        #endregion

        void proc()
        {
            JContext jc = JContext.Current;
            HttpContext context = jc.Context;

            // set a ajax request token
            jc.IsAjaxRequest = true;

            // get querystring
            string qs = context.Request.Params["querystring"];
            if (StringUtil.HasText(qs))
            {
                qs = qs.TrimStart('?');

                jc.QueryString.Add(StringUtil.DelimitedEquation2NVCollection("&", qs));
            }

            if (context.Request.UrlReferrer != null)
            {
                UrlMappingModule module = UrlMappingModule.Instance;
                if (module != null)
                {
                    UrlMappingItem mapping = null;
                    jc.QueryString.Add(module.GetMappedQueryString(context.Request.UrlReferrer.AbsolutePath, out mapping));

                    if (mapping != null)
                    {
                        NavigationInfo navi = new NavigationInfo();
                        navi.Set(mapping, UrlMappingModule.GetUrlRequested(context.Request.UrlReferrer.AbsolutePath));

                        jc.Navigation = navi;

                        // fire url matched event
                        module.OnUrlMatched();
                    }
                }
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
                    AjaxClass c = config.FindClass(classId, jc.Navigation.Id);

                    m = config.FindMethod(c, methodName);

                    if (string.Equals("Post", m.AjaxType, StringComparison.InvariantCultureIgnoreCase))
                        cacheMinutes = -1;
                    else if (StringUtil.HasText(m.CacheTest))
                        cacheMinutes = methodJsonArgs.Equals(m.CacheTest) ? cacheMinutes : -1;

                    // before execute
                    BeforeExecuteEventArgs e = new BeforeExecuteEventArgs() { JContext = jc, TypeName = c.Key, MethodName = m.MethodName };
                    OnBeforeExecute(e);
                    if (e.PreventDefault)
                    {
                        result = e.ReturnValue;
                        goto response;
                    }

                    if (c.Type != null)
                        result = m.Invoke(c.Type, methodJsonArgs);
                    else
                        result = m.Invoke(c.TypeString, methodJsonArgs);
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger<AjaxController>().Error("ajax handler error." + ExceptionUtil.WriteException(ex));

                    AjaxServerException ajaxEx = null;
                    if (m != null)
                        ajaxEx = m.Exception;

                    if (ajaxEx != null)
                        result = ajaxEx.ToJson();
                    else
                        result = null;
                }
            }

            goto response;

        response:

            OnAfterExecute(result);

            ResponseUtil.OutputJson(context.Response, result, cacheMinutes, jsonp);
            ContentType = context.Response.ContentType;
        }
    }
}
