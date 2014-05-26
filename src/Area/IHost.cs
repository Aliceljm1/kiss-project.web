using System.Collections.Generic;

namespace Kiss.Web
{
    /// <summary>
    /// Classes implementing this interface knows about available areas 
    /// and which one is the current based on the context.
    /// </summary>
    public interface IHost
    {
        /// <summary>
        /// The current site based on the request's host header information.         
        /// </summary>
        IArea CurrentArea { get; }

        IList<IArea> AllAreas { get; }

        IArea GetByAreaKey(string siteKey);
    }

    public class Host : IHost
    {
        public IArea CurrentArea
        {
            get { return AreaConfig.Instance; }
        }

        public IList<IArea> AllAreas
        {
            get { return new List<IArea>() { CurrentArea }; }
        }

        public IArea GetByAreaKey(string areaKey)
        {
            return CurrentArea;
        }
    }
}
