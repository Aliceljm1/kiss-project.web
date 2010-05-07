using System;
using System.Web;

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// represents a result that performs a redirection given some URI
    /// </summary>
    public class RedirectResult : ActionResult
    {
        public RedirectResult( string url )
        {
            if( String.IsNullOrEmpty( url ) )
            {
                throw new ArgumentException( "url" );
            }

            Url = url;
        }

        public string Url
        {
            get;
            private set;
        }

        public override void ExecuteResult( IControllerContext context )
        {
            if( context == null )
            {
                throw new ArgumentNullException( "context" );
            }

            HttpContext.Current.Response.Redirect( Url, true );
        }

    }
}
