
using System;
using System.Web;

namespace Kiss.Web.Mvc
{
    public class HttpUnauthorizedResult : ActionResult
    {
        public override void ExecuteResult(JContext jc)
        {
            if (jc == null)
            {
                throw new ArgumentNullException("context");
            }

            // 401 is the HTTP status code for unauthorized access - setting this
            // will cause the active authentication module to execute its default
            // unauthorized handler
            jc.Context.Response.StatusCode = 401;
        }
    }
}
