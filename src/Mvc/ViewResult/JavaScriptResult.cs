#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-12-01
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-12-01		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System;
using System.Web;

namespace Kiss.Web.Mvc
{
    public class JavaScriptResult : ActionResult
    {
        public string Script
        {
            get;
            set;
        }

        public override void ExecuteResult(IControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpResponse response = HttpContext.Current.Response;
            //response.ContentType = "application/x-javascript";

            if (Script != null)
            {
                response.Write("<script type='text/javascript'>");
                response.Write(Script);
                response.Write("</script>");
                response.End();
            }
        }
    }
}
