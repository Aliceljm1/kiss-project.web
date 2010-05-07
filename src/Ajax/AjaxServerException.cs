#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-11-11
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-11-11		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

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
