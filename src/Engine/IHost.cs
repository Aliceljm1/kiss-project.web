
namespace Kiss.Web
{
    /// <summary>
    /// Classes implementing this interface knows about available Sites 
    /// and which one is the current based on the context.
    /// </summary>
    public interface IHost
    {
        /// <summary>
        /// The current site based on the request's host header information.         
        /// </summary>
        ISite CurrentSite { get; }
    }
}
