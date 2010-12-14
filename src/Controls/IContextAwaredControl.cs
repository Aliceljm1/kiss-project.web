
namespace Kiss.Web.Controls
{
    /// <summary>
    /// use this interface to get site info
    /// </summary>
    public interface IContextAwaredControl
    {
        ISite CurrentSite { get; set; }
    }
}
