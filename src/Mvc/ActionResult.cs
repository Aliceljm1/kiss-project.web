
namespace Kiss.Web.Mvc
{
    /// <summary>
    /// mvc action result
    /// </summary>
    public abstract class ActionResult
    {
        /// <summary>
        /// execute mvc action result
        /// </summary>
        /// <param name="context"></param>
        public abstract void ExecuteResult( IControllerContext context );
    }
}
