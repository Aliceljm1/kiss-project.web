namespace Kiss.Web.Ajax
{
    public class AjaxServerException
    {
        public AjaxServerExceptionAction Action { get; set; }

        public string Parameter { get; set; }
    }

    public enum AjaxServerExceptionAction
    {
        JSEval,
        JSMethod,
        returnValue,
    }
}
