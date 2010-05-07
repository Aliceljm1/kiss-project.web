#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-10-10
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-10-10		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// use this interface to invoke method of IController
    /// </summary>
    public interface IActionInvoker
    {
        /// <summary>
        /// invoke
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        bool InvokeAction(IControllerContext controllerContext);
    }
}
