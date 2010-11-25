using System;
using System.Collections.Generic;
using System.Web;
using Kiss.Plugin;
using Kiss.Utils;

namespace Kiss.Web
{
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
                        datas = new Dictionary<string, object>();

                        foreach (var item in Plugins.GetPlugins<ContextDataAttribute>())
                        {
                            object context = Activator.CreateInstance(item.Decorates);
                            if (context == null)
                                continue;

                            datas[item.Key] = context;
                        }

                    }
                }

                return datas;
            }
        }
    }

    /// <summary>
    /// mark a type to context data. can use it like this $!context_data_key.***
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ContextDataAttribute : PluginAttribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string key;

        // This is a positional argument
        public ContextDataAttribute(string key)
        {
            this.key = key;
        }

        public string Key
        {
            get { return key; }
        }
    }

    [ContextData("utils")]
    class ContextDataUtils
    {
        public static string[] split(string str)
        {
            return StringUtil.Split(str, StringUtil.Comma, true, true);
        }

        public static string trim(string str, int maxlength)
        {
            return StringUtil.Trim(str, maxlength);
        }

        public static DateTime now { get { return DateTime.Now; } }

        public static string htmlEncode(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            return HttpUtility.HtmlEncode(str);
        }

        public static string urlEncode(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            return HttpUtility.UrlEncode(str);
        }
    }
}
