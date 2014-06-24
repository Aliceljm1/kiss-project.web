
namespace Kiss.Web.Controls
{
    /// <summary>
    /// use this interface to get site info
    /// </summary>
    public interface IContextAwaredControl
    {
        IArea CurrentSite { get; set; }
    }
}
