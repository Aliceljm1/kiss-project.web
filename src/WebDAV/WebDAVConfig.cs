using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kiss.Config;

namespace Kiss.Web.WebDAV
{
    [ConfigNode("webdav")]
    public class WebDAVConfig : ConfigBase
    {
        public static WebDAVConfig Instance { get { return GetConfig<WebDAVConfig>(); } }

        [ConfigProp("defaultCacheTTL", ConfigPropAttribute.DataType.Int, DefaultValue = 0)]
        public int DefaultCacheTTL { get; private set; }
    }
}
