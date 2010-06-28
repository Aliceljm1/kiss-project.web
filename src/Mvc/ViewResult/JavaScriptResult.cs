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

        public override void ExecuteResult(JContext jc)
        {
            if (jc == null)
            {
                throw new ArgumentNullException("jc");
            }

            HttpResponse response = jc.Context.Response;
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
