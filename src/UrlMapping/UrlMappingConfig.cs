using System.Xml;
using Kiss.Config;
using Kiss.Utils;

namespace Kiss.Web.UrlMapping
{
    /// <summary>
    /// url重定向配置基类
    /// </summary>
    [ConfigNode( "urlMapping", Desc = "url重定向" )]
    public class UrlMappingConfig : ConfigBase
    {
        #region fields / properties

        /// <summary>
        /// 配置provider类型
        /// </summary>
        [ConfigProp( "providerType", DefaultValue = "Kiss.Web.UrlMapping.XmlUrlMappingProvider, Kiss.Web" )]
        public string ProviderType { get; protected set; }

        /// <summary>
        /// url不匹配的操作
        /// </summary>
        [ConfigProp( "noMatchAction", DefaultValue = "PassThrough", Options = "PassThrough,ThrowException,Return404,Redirect" )]
        public string NoMatchActionStr { get; protected set; }

        /// <summary>
        /// url不匹配的操作
        /// </summary>
        public NoMatchAction NoMatchAction { get; private set; }

        /// <summary>
        /// url不匹配重定向url
        /// </summary>
        [ConfigProp( "noMatchRedirectUrl", DefaultValue = "404.html" )]
        public string NoMatchRedirectUrl { get; protected set; }

        [ConfigProp( "automaticallyUpdateFormAction", ConfigPropAttribute.DataType.Boolean, DefaultValue = true )]
        public bool AutoUpdateFormAction { get; protected set; }

        [ConfigProp( "incomingQueryStringBehavior", DefaultValue = "PassThrough", Options = "PassThrough,Ingore,Include" )]
        public string IncomingQueryStringBehaviorStr { get; protected set; }
        public IncomingQueryStringBehavior IncomingQueryStringBehavior { get; private set; }

        [ConfigProp( "ignoreExtensions", DefaultValue = ".css .jpg .jpeg .gif .png .axd .htm .html .js .xml .swf .gallery .icon .ico .flv .zip .rar" )]
        public string IgnoreExtensions { get; protected set; }

        [ConfigProp( "allowExtensions" )]
        public string AllowExtensions { get; protected set; }

        [ConfigProp( "urlProcessingEvent", DefaultValue = "AuthorizeRequest", Options = "BeginRequest,AuthenticateRequest,AuthorizeRequest" )]
        public string UrlProcessingEventStr { get; protected set; }
        public UrlProcessingEvent UrlProcessingEvent { get; private set; }

        /// <summary>
        /// 配置文件路径
        /// </summary>
        [ConfigProp( "urlMappingFile", DefaultValue = "~/App_Data/routes.config", Desc = "配置文件路径" )]
        public string UrlMappingFile { get; protected set; }

        /// <summary>
        /// 是否启用文件缓存依赖
        /// </summary>
        [ConfigProp( "useDependency", ConfigPropAttribute.DataType.Boolean, DefaultValue = true, Desc = "是否启用文件缓存依赖" )]
        public bool UseDependency { get; protected set; }

        internal IUrlMappingProvider Provider { get; set; }

        #endregion

        /// <summary>
        /// 获取配置实例
        /// </summary>
        /// <returns></returns>
        public static UrlMappingConfig Instance
        {
            get { return GetConfig<UrlMappingConfig>(); }
        }

        #region override

        protected override void LoadValuesFromConfigurationXml( XmlNode node )
        {
            base.LoadValuesFromConfigurationXml( node );

            NoMatchAction = StringEnum<NoMatchAction>.SafeParse( NoMatchActionStr );
            IncomingQueryStringBehavior = StringEnum<IncomingQueryStringBehavior>.SafeParse( IncomingQueryStringBehaviorStr );
            UrlProcessingEvent = StringEnum<UrlProcessingEvent>.SafeParse( UrlProcessingEventStr );
        }

        #endregion
    }
}
