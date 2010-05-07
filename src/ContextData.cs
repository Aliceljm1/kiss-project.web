#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-05-26
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-05-26		zhli Comment Created
//+-------------------------------------------------------------------+
//+ 2009-07-09		zhli remove ContextDataAttribute
//+-------------------------------------------------------------------+
//+ 2009-07-28		zhli 增加了配置标签
//+-------------------------------------------------------------------+
#endregion

using System;
using System.Collections.Generic;
using System.Xml;
using Kiss.Config;
using Kiss.Utils;

namespace Kiss.Web
{
    /// <summary>
    /// 上下文数据的配置
    /// </summary>
    [ConfigNode("contextData", Desc = "上下文数据")]
    public class ContextDataConfig : ConfigBase
    {
        /// <summary>
        /// 配置实例
        /// </summary>
        public static ContextDataConfig Instance
        {
            get
            {
                return GetConfig<ContextDataConfig>();
            }
        }

        /// <summary>
        /// 配置的名称、类型
        /// </summary>
        [ConfigProp("datas", ConfigPropAttribute.DataType.Unknown, Desc = "key+type")]
        public Dictionary<string, string> Dict { get; private set; }

        protected override void LoadValuesFromConfigurationXml(XmlNode node)
        {
            base.LoadValuesFromConfigurationXml(node);

            Dict = new Dictionary<string, string>();

            XmlNodeList nodes = node.SelectNodes("datas/add");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (XmlNode n in nodes)
                {
                    string key = XmlUtil.GetStringAttribute(n, "key", string.Empty);
                    string type = XmlUtil.GetStringAttribute(n, "type", string.Empty);

                    if (StringUtil.IsNullOrEmpty(key) || StringUtil.IsNullOrEmpty(type))
                        continue;
                    Dict[key] = type;
                }
            }
        }
    }

    internal class ContextData
    {
        private static readonly object obj = new object();
        private static Dictionary<string, object> datas;
        public static Dictionary<string, object> Datas
        {
            get
            {
                if (datas != null)
                    return datas;

                lock (obj)
                {
                    if (datas == null)
                    {
                        ContextDataConfig config = ContextDataConfig.Instance;

                        datas = new Dictionary<string, object>();

                        foreach (string key in config.Dict.Keys)
                        {
                            string type = config.Dict[key];

                            try
                            {
                                Type t = Type.GetType(type);
                                if (t == null)
                                    continue;

                                object context = Activator.CreateInstance(t);
                                if (context == null)
                                    continue;

                                datas[key] = context;
                            }
                            catch (Exception ex)
                            {
                                throw new WebException(string.Format("init context data failed! type:{0}", type), ex);
                            }
                        }
                    }
                }

                return datas;
            }
        }
    }
}
