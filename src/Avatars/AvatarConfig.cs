using System.Collections.Generic;
using System.Xml;
using Gala.Config;
using Gala.Storage;

namespace Gala.Web.Avatars
{
    [ConfigNode ( "avatar", Desc = "头像" )]
    public class AvatarConfig : ConfigWithProviders
    {
        private List<StorageConfiguration> _storageSettings = new List<StorageConfiguration> ( );
        public List<StorageConfiguration> StorageSettings
        {
            get { return _storageSettings; }
        }

        [ConfigProp ( "bigsize", ConfigPropAttribute.DataType.String, DefaultValue = "size200", Desc = "大尺寸" )]
        public string BigSize { get; private set; }

        [ConfigProp ( "smallsize", ConfigPropAttribute.DataType.String, DefaultValue = "size50", Desc = "小尺寸" )]
        public string CutSize { get; private set; }

        [ConfigProp ( "cutsize", ConfigPropAttribute.DataType.String, DefaultValue = "size120", Desc = "切割尺寸" )]
        public string SmallSize { get; private set; }

        [ConfigProp ( "defaultAvatar", ConfigPropAttribute.DataType.String, DefaultValue = "avatar.jpg", Desc = "默认头像" )]
        public string DefaultAvatar { get; private set; }

        public static AvatarConfig Instance
        {
            get
            {
                return GetConfig<AvatarConfig> ( );
            }
        }

        protected override void LoadConfigsFromConfigurationXml ( XmlNode node )
        {
            base.LoadConfigsFromConfigurationXml ( node );

            // get storage settings       
            XmlNodeList xmlList = node.SelectNodes ( "storages/add" );
            if ( xmlList != null && xmlList.Count > 0 )
            {
                foreach ( XmlNode sn in xmlList )
                {
                    StorageConfiguration storageSettings = StorageConfiguration.GetConfig<StorageConfiguration> ( sn, false );
                    _storageSettings.Add ( storageSettings );
                }
            }
        }
    }
}
