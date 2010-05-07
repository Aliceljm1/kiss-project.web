
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using System.Xml;
using Kiss.Config;
using Kiss.Utils;

namespace Kiss.Web.Ajax
{
    /// <summary>
    /// ajax的配置
    /// </summary>
    [ConfigNode( "ajax", Desc = "ajax" )]
    public class AjaxConfiguration : ConfigBase
    {
        private const string CACHEKEY = "_ajaxclasslist_";

        #region fields / properties

        [ConfigProp( "configFile", ConfigPropAttribute.DataType.String, DefaultValue = "~/App_Data/ajaxConfig.xml", Desc = "ajax配置文件路径" )]
        public string ConfigFile { get; private set; }

        private XmlNamespaceManager nsmgr;

        private List<AjaxClass> AjaxClassList
        {
            get
            {
                List<AjaxClass> classList = HttpRuntime.Cache.Get( CACHEKEY ) as List<AjaxClass>;

                if( classList == null )
                {
                    string filepath = HttpContext.Current.Server.MapPath( ConfigFile );

                    classList = ReadConfig( filepath );

                    HttpRuntime.Cache.Insert( CACHEKEY, classList, new CacheDependency( filepath ) );
                }

                return classList;
            }
        }

        public static readonly Dictionary<Type, AjaxClass> ControllerAjax = new Dictionary<Type, AjaxClass>();
        private static readonly Dictionary<string, AjaxMethod> _methodsCache = new Dictionary<string, AjaxMethod>();

        #endregion

        #region GetConfig

        public static AjaxConfiguration GetConfig()
        {
            return GetConfig<AjaxConfiguration>();
        }

        #endregion
        
        #region Methods

        public AjaxClass FindClass( string className )
        {
            if( string.Equals( className, "gAjax", StringComparison.InvariantCultureIgnoreCase ) )
            {
                Type t = JContext.Current.ControllerContext.CurrentControllerType;
                if( t != null && AjaxConfiguration.ControllerAjax.ContainsKey( t ) )
                    return AjaxConfiguration.ControllerAjax[ t ];
            }

            foreach( AjaxClass c in AjaxClassList )
            {
                if( string.Equals( c.Id, className ) )
                    return c;
            }

            throw new AjaxException( "can't find ajax class. className: " + className );
        }

        public AjaxMethod FindMethod( AjaxClass c, string methodName )
        {
            string cachekey = string.Format( "{0}.{1}", c.Key, methodName );
            if( _methodsCache.ContainsKey( cachekey ) )
                return _methodsCache[ cachekey ];

            foreach( AjaxMethod m in c.Methods )
            {
                if( string.Equals( m.MethodName, methodName ) )
                {
                    _methodsCache[ cachekey ] = m;
                    return m;
                }
            }

            throw new AjaxException( "can't find ajax method. className:" + c.TypeString + " methodName: " + methodName );
        }

        #endregion

        #region helper

        private List<AjaxClass> ReadConfig( string file )
        {
            List<AjaxClass> list = new List<AjaxClass>();

            XmlDocument xml = new XmlDocument();
            xml.Load( file );
            nsmgr = new XmlNamespaceManager( xml.NameTable );
            nsmgr.AddNamespace( "ajax", xml.DocumentElement.NamespaceURI );

            foreach( XmlNode classNode in xml.DocumentElement.SelectNodes( "ajax:class", nsmgr ) )
            {
                list.Add( GetClass( classNode ) );
            }

            return list;
        }

        private AjaxClass GetClass( XmlNode node )
        {
            AjaxClass c = new AjaxClass();
            c.Id = XmlUtil.GetStringAttribute( node, "id", Guid.NewGuid().ToString() );
            c.TypeString = XmlUtil.GetStringAttribute( node, "type", string.Empty );

            try
            {
                TypeRegistry.RegisterType( c.TypeString, c.TypeString );
            }
            catch( Exception ex )
            {
                throw new AjaxException( "ajax config error. type:" + c.TypeString + " not found", ex );
            }

            foreach( XmlNode n in node.SelectNodes( "ajax:method", nsmgr ) )
            {
                c.Methods.Add( GetMethod( n ) );
            }

            return c;
        }

        private AjaxMethod GetMethod( XmlNode node )
        {
            AjaxMethod m = new AjaxMethod();

            m.MethodName = XmlUtil.GetStringAttribute( node, "name", string.Empty );
            // cache an hour
            m.CacheMinutes = XmlUtil.GetIntAttribute( node, "cache", 60 );
            m.CacheTest = XmlUtil.GetStringAttribute( node, "cacheTest", string.Empty );
            m.AjaxType = XmlUtil.GetStringAttribute( node, "type", "Get" );
            foreach( XmlNode n in node.SelectNodes( "ajax:param", nsmgr ) )
            {
                m.Params.Add( GetParam( n ) );
            }

            m.Exception = GetException( node.SelectSingleNode( "ajax:onException", nsmgr ) );

            return m;
        }

        private AjaxParam GetParam( XmlNode node )
        {
            AjaxParam p = new AjaxParam();

            p.ParamName = XmlUtil.GetStringAttribute( node, "name", string.Empty );
            p.ParamType = XmlUtil.GetStringAttribute( node, "type", string.Empty );

            return p;
        }

        private AjaxServerException GetException( XmlNode node )
        {
            if( node == null )
                return null;

            AjaxServerException e = new AjaxServerException();

            e.Action = StringEnum<AjaxServerExceptionAction>.SafeParse( XmlUtil.GetStringAttribute( node, "action", "JSEval" ) );
            e.Parameter = XmlUtil.GetStringAttribute( node, "parameter", string.Empty );

            return e;
        }

        #endregion
    }
}
