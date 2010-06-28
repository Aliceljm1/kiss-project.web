using System;
using System.Web;
using System.Threading;

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// represents a result that performs a redirection given some URI
    /// </summary>
    public class RedirectResult : ActionResult
    {
        public RedirectResult(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                throw new ArgumentException("url");
            }

            Url = url;
        }

        public string Url
        {
            get;
            private set;
        }

        public override void ExecuteResult(JContext jc)
        {
            if (jc == null)
            {
                throw new ArgumentNullException("jc");
            }

            jc.Context.Response.Redirect(Url, true);
        }
    }
}
