using System;
using System.Collections.Generic;
using System.Text;
using Gala.Caching;
using Gala.Core;

namespace Gala.Web.Avatars
{
    public static class AvatarManager
    {
        public static Avatar Get ( int id )
        {
            string key = string.Format ( "avatar:{0}" , id );

            Avatar avatar = JCache.Get<Avatar> ( key );
            if ( avatar == null )
            {
                avatar = AvatarDataProvider.Instance.GetAvatar ( id );

                if ( avatar != null )
                    JCache.Insert ( key , avatar );
            }

            return avatar;
        }

        public static Avatar GetByUserName ( string username )
        {
            string key = GetCacheKeyByUserName ( username );

            Avatar a = JCache.Get<Avatar> ( key );
            if ( a == null )
            {
                a = AvatarDataProvider.Instance.GetCurrentAvatar ( username );
                if ( a != null )
                    JCache.Insert ( key , a );
            }

            return a;
        }

        public static void Save ( Avatar avatar )
        {
            DataProviderAction action = DataProviderAction.NotSet;

            if ( avatar.Id == 0 )
            {
                action = DataProviderAction.Create;
                avatar.Step = AvatarStep.Step1;

                avatar.DateCreated = DateTime.Now;

                AvatarDataProvider.Instance.CreateUpdateDeleteAvatar ( avatar , action );

                avatar.StorageProvider.Save ( avatar );

                AvatarDataProvider.Instance.CreateUpdateDeleteAvatar ( avatar , DataProviderAction.Update );
            }
            else
            {
                action = DataProviderAction.Update;

                avatar.Step = AvatarStep.Step2;
                avatar.StorageProvider.Save ( avatar );

                AvatarDataProvider.Instance.CreateUpdateDeleteAvatar ( avatar , action );
            }

            JCache.Insert ( GetCacheKeyByUserName ( avatar.UserName ) , avatar );
        }

        public static string GetAvatarUrl ( string username , string sizename )
        {
            Avatar a = AvatarManager.GetByUserName ( username );

            return GetAvatarUrl ( a , sizename );
        }

        public static string GetAvatarUrl ( Avatar avatar , string sizename )
        {
            string src;
            if ( avatar == null || avatar.Step == AvatarStep.Step1 )
                src = AvatarConfig.Instance.DefaultAvatar;
            else
                src = avatar.GetUrl ( sizename ?? "size50" );

            return src;
        }

        private static string GetCacheKeyByUserName ( string username )
        {
            return string.Format ( "user.avatar:{0}" , username );
        }
    }
}
