using System.Drawing;
using System.IO;
using Gala.Storage;
using Gala.Utils;
using Gala.Utils.Imaging;

namespace Gala.Web.Avatars
{
    public class DiskStorageProvider : Gala.Storage.DiskStorageProvider
    {
        protected override string GetStorageFilename<T> ( IStorage<T> storage )
        {
            string extension = Path.GetExtension ( storage.FileName );
            return storage.Id + extension;
        }

        protected override string CalculateStorageLocation<T> ( IStorage<T> storage )
        {
            Avatar a = storage as Avatar;
            if ( a == null )
                return base.CalculateStorageLocation<T> ( storage );
            return a.UserName;
        }

        public override void Save<T> ( IStorage<T> storage )
        {
            Avatar a = storage as Avatar;
            if ( a == null )
                return;

            string name = Path.GetFileNameWithoutExtension ( a.FileName );
            a.Desc = a.FriendlyFileName = name;
            a.FileName = GetStorageFilename ( storage );
            a.ContentType = ImageUtil.GetContentType ( a.FileName );

            string path = GetStoragePath ( a , true );

            switch ( a.Step )
            {
                case AvatarStep.Step1:
                    // save orignal file       
                    FileUtil.CreateDirectory ( Path.GetDirectoryName ( path ) );
                    using ( FileStream fs = new FileStream ( path , FileMode.Create ) )
                    {
                        byte[] buffer = new byte[ a.ContentSize ];
                        a.Content.Read ( buffer , 0 , ( int ) a.ContentSize );

                        fs.Write ( buffer , 0 , ( int ) a.ContentSize );
                    }
                    a.Content.Close ( );
                    a.Content.Dispose ( );
                    a.Content = null;
                    break;
                case AvatarStep.Step2:
                    Gala.Storage.ThumbnailSettings settings = a.StorageConfig.ThumbnailSettings;

                    AvatarConfig config= AvatarConfig.Instance;
                    ThumbnailSize size;

                    // save big size img based on orignal image
                    using ( Image image = Image.FromFile ( path ) )
                    {
                        a.Width = image.Width;
                        a.Height = image.Height;

                        size = settings.Sizes[ config.BigSize ];
                        Size realSize = ImageUtil.GetThumbnailImageSize ( image.Width , image.Height , size.Width , size.Height );
                        using ( Image thumbnail = settings.GetThumbnailImage ( image , realSize.Width , realSize.Height ) )
                        {
                            ImageUtil.SaveImage ( thumbnail ,
                                settings.GetThumbnailStoragePath ( a , thumbnail.Width , thumbnail.Height , true ) ,
                                ImageUtil.GetImageFormat ( a.FileName ) );

                            a.SetThumbnailSize ( config.BigSize , new ThumbnailSize ( thumbnail.Width , thumbnail.Height ) );
                        }
                    }

                    // save cut file
                    size = settings.Sizes[ config.CutSize ];
                    ImageUtil.SaveCutPic ( path ,
                        settings.GetThumbnailStoragePath ( a , size.Width , size.Height , true ) ,
                        0 ,
                        0 , a.Cut.ToWidth , a.Cut.ToHeight , a.Cut.Left , a.Cut.Top , a.Cut.FromWidth , a.Cut.FromHeight );
                    a.SetThumbnailSize ( config.CutSize , size );

                    // save small size img based on cut info

                    using ( Image image = Image.FromFile ( settings.GetThumbnailStoragePath ( a , size.Width , size.Height , true ) ) )
                    {
                        size = settings.Sizes[ config.SmallSize ];
                        Size realSize = ImageUtil.GetThumbnailImageSize ( image.Width , image.Height , size.Width , size.Height );
                        using ( Image thumbnail = settings.GetThumbnailImage ( image , realSize.Width , realSize.Height ) )
                        {
                            ImageUtil.SaveImage ( thumbnail ,
                                settings.GetThumbnailStoragePath ( a , thumbnail.Width , thumbnail.Height , true ) ,
                                ImageUtil.GetImageFormat ( a.FileName ) );

                            a.SetThumbnailSize ( config.SmallSize , new ThumbnailSize ( thumbnail.Width , thumbnail.Height ) );
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public override void Delete<T> ( IStorage<T> storage )
        {
            Avatar a = storage as Avatar;
            if ( a == null )
                return;

            Gala.Storage.ThumbnailSettings settings =  a.StorageConfig.ThumbnailSettings;
            foreach ( string sizeName in settings.Sizes.Keys )
            {
                ThumbnailSize size = a.GetThumbnailSize ( sizeName );
                settings.Delete ( storage , size.Width , size.Height );
            }
            base.Delete ( storage );
        }
    }
}
