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
        public static readonly ContextDataUtils Instance = new ContextDataUtils();

        public static string[] split(string str)
        {
            return StringUtil.Split(str, StringUtil.Comma, true, true);
        }

        public static string trim(string str, int maxlength)
        {
            return StringUtil.Trim(str, maxlength);
        }

        private static readonly StringUtil _str = new StringUtil();
        public static StringUtil str { get { return _str; } }

        private static readonly DictSchema _schema = new DictSchema();
        public static DictSchema schema { get { return _schema; } }

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

        public static Url url(string url)
        {
            return new Url(url);
        }

        public static Url updateQuery(string k, string v)
        {
            return new Url(HttpContext.Current.Request.Url.PathAndQuery).UpdateQuery(k, v);
        }

        public static bool hasText(string str)
        {
            return StringUtil.HasText(str);
        }

        public static DateTime toDateTime(string obj)
        {
            if (string.IsNullOrEmpty(obj)) return DateTime.Now;

            return StringUtil.ToDateTime(obj);
        }

        public bool isDateTime(string str)
        {
            if (StringUtil.IsNullOrEmpty(str)) return false;

            DateTime dt;

            bool valid = DateTime.TryParse(str, out dt);

            if (valid)
            {
                valid = dt < DateTime.MaxValue && dt > DateTime.MinValue;
            }

            return valid;
        }

        public int toInt(string str)
        {
            return str.ToInt();
        }
    }
}
