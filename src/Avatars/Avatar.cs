using System;
using System.Data;
using Gala.Storage;
using Gala.Utils;

namespace Gala.Web.Avatars
{
    [Serializable]
    public class Avatar : StorageObj<int>
    {
        #region props

        public int Width { get; set; }
        public int Height { get; set; }
        public int StorageKey { get; set; }
        public AvatarStep Step { get; set; }
        public string UserName { get; set; }

        [NonSerialized]
        private CutInfo _cutInfo;
        public CutInfo Cut { get { return _cutInfo; } set { _cutInfo = value; } }

        public StorageConfiguration StorageConfig
        {
            get
            {
                AvatarConfig config = AvatarConfig.Instance;
                int index = Math.Max ( config.StorageSettings.Count - 1, StorageKey );

                return config.StorageSettings[ index ];
            }
        }

        [NonSerialized]
        private DiskStorageProvider _storageProvider;
        public DiskStorageProvider StorageProvider
        {
            get
            {
                if ( _storageProvider == null )
                    _storageProvider = DiskStorageProvider.Instance ( StorageConfig ) as DiskStorageProvider;
                return _storageProvider;
            }
        }

        public string ImageUrl
        {
            get
            {
                return StorageProvider.GetHttpDirectPath ( this );
            }
        }

        public string GetUrl ( string sizeName )
        {
            Gala.Storage.ThumbnailSettings settings = StorageConfig.ThumbnailSettings;
            ThumbnailSize size = GetThumbnailSize ( sizeName );
            if ( size.Height == 0 || size.Width == 0 )// orignal file
                return ImageUrl;
            return settings.GetThumbnailHttpPath ( this, size.Width, size.Height );
        }

        #endregion

        #region extended properties

        private const string X_FORMAT  = "{0}-X";
        private const string Y_FORMAT  = "{0}-Y";

        private ExtendedAttributes _extAttrs = new ExtendedAttributes ( );
        public ExtendedAttributes ExtAttrs { get { return _extAttrs; } }
        public int GetThumbnailX ( string sizeName )
        {
            return GetThumbnail ( sizeName, X_FORMAT );
        }

        public int GetThumbnailY ( string sizeName )
        {
            return GetThumbnail ( sizeName, Y_FORMAT );
        }

        public ThumbnailSize GetThumbnailSize ( string sizeName )
        {
            return new ThumbnailSize ( GetThumbnailX ( sizeName ),
                GetThumbnailY ( sizeName ) );
        }

        public void SetThumbnailSize ( string sizeName, ThumbnailSize size )
        {
            SetThumbnail ( sizeName, X_FORMAT, size.Width );
            SetThumbnail ( sizeName, Y_FORMAT, size.Height );
        }

        public int GetThumbnail ( string sizeName, string xOry )
        {
            string value = ExtAttrs.GetExtendedAttribute ( GetThumbnailKey ( sizeName, xOry ) );

            if ( !string.IsNullOrEmpty ( value ) )
                return int.Parse ( value );

            return 0;
        }

        public void SetThumbnail ( string sizeName, string xOry, object value )
        {
            ExtAttrs.SetExtendedAttribute ( GetThumbnailKey ( sizeName, xOry ),
                 value.ToString ( ) );
        }

        public static string GetThumbnailKey ( string sizeName, string xOry )
        {
            return string.Format ( xOry, sizeName );
        }

        #endregion

        public Avatar ( )
        {
        }

        public Avatar ( IDataReader rdr )
            : base ( rdr )
        {
            StorageKey = DataUtil.SafePopulate<int> ( rdr, "StorageKey" );
            Width = DataUtil.SafePopulate<int> ( rdr, "Width" );
            Height = DataUtil.SafePopulate<int> ( rdr, "Height" );
            Step = DataUtil.SafePopulate<AvatarStep> ( rdr, "Step" );
            UserName = DataUtil.SafePopulate<string> ( rdr, "UserName" );

            ExtAttrs.SetData ( DataUtil.SafePopulate<string> ( rdr, "PropertyName" ), DataUtil.SafePopulate<string> ( rdr, "PropertyValue" ) );
        }
    }

    public enum AvatarStep
    {
        Step1 = 1,
        Step2 = 2,
    }
}
