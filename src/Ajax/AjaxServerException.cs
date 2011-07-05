namespace Kiss.Web.Ajax
{
    public class AjaxServerException
    {
        public AjaxServerExceptionAction Action { get; set; }

        public string Parameter { get; set; }

        public object ToJson()
        {
            return new { __AjaxException = new { action = Action, parameter = Parameter } };
        }
    }

    public enum AjaxServerExceptionAction
    {
        JSEval,
        JSMethod,
        returnValue,
    }
}
