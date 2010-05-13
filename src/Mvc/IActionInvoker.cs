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
        /// <returns></returns>
        bool InvokeAction(JContext jc);

        bool IsAsync(JContext jc);
    }
}
