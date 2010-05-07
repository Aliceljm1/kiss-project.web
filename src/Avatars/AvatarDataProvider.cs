using System.Configuration.Provider;
using Gala.Core;

namespace Gala.Web.Avatars
{
    public abstract class AvatarDataProvider : ProviderBase
    {
        #region Instance

        private static AvatarDataProvider _instance;

        public static AvatarDataProvider Instance
        {
            get { return _instance; }
        }

        static AvatarDataProvider ( )
        {
            AvatarConfig config = AvatarConfig.Instance;

            _instance = ProviderHelper.CreateProvider ( config ) as AvatarDataProvider;
        }

        #endregion

        public abstract Avatar GetAvatar ( int id );
        public abstract Avatar GetCurrentAvatar ( string username );
        public abstract void CreateUpdateDeleteAvatar ( Avatar a, DataProviderAction action );
    }
}
