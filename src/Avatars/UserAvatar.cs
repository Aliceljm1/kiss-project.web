using System.Web.UI;

namespace Gala.Web.Avatars
{
    public class UserAvatar : Control
    {
        public string SizeName { get; set; }
        public string UserName { get; set; }
        public string ImgId { get; set; }
        public string ImgClass { get; set; }

        private Avatar _avatar;
        public Avatar Avatar
        {
            get
            {
                if ( _avatar == null )
                {
                    _avatar = AvatarManager.GetByUserName ( UserName ?? JContext.Current.UserName );
                }
                return _avatar;
            }
            set
            {
                _avatar = value;
            }
        }

        protected override void Render ( HtmlTextWriter writer )
        {
            base.Render ( writer );

            writer.Write ( string.Format ( "<img src='{0}' {1} {2}/>" ,
                AvatarManager.GetAvatarUrl ( Avatar , SizeName ) ,
                string.IsNullOrEmpty ( ImgId ) ? string.Empty : string.Format ( "Id={0}" , ImgId ) ,
                string.IsNullOrEmpty ( ImgClass ) ? string.Empty : string.Format ( "class={0}" , ImgClass ) ) );
        }
    }
}
