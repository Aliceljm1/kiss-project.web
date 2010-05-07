
using System;
using System.Web;

namespace Kiss.Web.Mvc
{
    public class HttpUnauthorizedResult : ActionResult
    {
        public override void ExecuteResult( IControllerContext context )
        {
            if( context == null )
            {
                throw new ArgumentNullException( "context" );
            }

            // 401 is the HTTP status code for unauthorized access - setting this
            // will cause the active authentication module to execute its default
            // unauthorized handler
            HttpContext.Current.Response.StatusCode = 401;
        }
    }
}
