namespace Kiss.Web.Ajax
{
    public class AjaxServerException
    {
        public AjaxServerExceptionAction Action { get; set; }

        private string parameter;
        public string Parameter { get { return parameter; } set { parameter = value.Replace("\r\n", "\\r\\n"); } }

        public object ToJson()
        {
            return new { __AjaxException = new { action = Action.ToString(), parameter = Parameter } };
        }
    }

    public enum AjaxServerExceptionAction
    {
        JSEval,
        JSMethod,
        returnValue,
    }
}
