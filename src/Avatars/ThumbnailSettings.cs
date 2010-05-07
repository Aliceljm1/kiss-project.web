using System.IO;
using System.Xml;
using Gala.Storage;

namespace Gala.Web.Avatars
{
    internal class ThumbnailSettings : Gala.Storage.ThumbnailSettings
    {
        public ThumbnailSettings ( XmlNode node )
            : base ( node )
        {
        }

        protected override string CalculateThumbnailStorageLocation<T> ( IStorage<T> storage )
        {
            Avatar a = storage as Avatar;
            if ( a == null )
            {
                return base.CalculateThumbnailStorageLocation ( storage );
            }

            return a.UserName;
        }

        protected override string GetThumbnailStorageFilename<T> ( IStorage<T> storage , int width , int height )
        {
            Avatar p= storage as Avatar;
            if ( p == null )
                return base.GetThumbnailStorageFilename ( storage , width , height );

            return string.Format ( "{0}_{1}x{2}{3}" ,
                Path.GetFileNameWithoutExtension ( storage.FileName ) ,
                width , height ,
                Path.GetExtension ( p.FileName ) );
        }
    }
}
