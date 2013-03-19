using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using Kiss.Utils;
using Kiss.Web.Ajax;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// user this class to declare ajax method
    /// </summary>
    [ParseChildren(typeof(AjaxServerClass)),
    PersistChildren(false)]
    public class Ajax : Control
    {
        private const string GAJAX = "__gAjaxRendered__";

        #region fields / props

        private Dictionary<string, List<string>> _methods = new Dictionary<string, List<string>>();

        /// <summary>
        /// current controller's ajax method name split by comma
        /// </summary>
        public string gAjaxMethods { get; set; }

        #endregion

        protected override void AddParsedSubObject(object obj)
        {
            if (obj is AjaxServerClass)
            {
                AjaxServerClass c = (AjaxServerClass)obj;

                if (_methods.ContainsKey(c.ClassName))
                {
                    if (_methods[c.ClassName] == null)
                        _methods[c.ClassName] = new List<string>();

                    _methods[c.ClassName].AddRange(c.Methods);
                }
                else
                {
                    _methods.Add(c.ClassName, c.Methods);
                }
            }
            else
            {
                base.AddParsedSubObject(obj);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            JContext jc = JContext.Current;

            if (!jc.IsAjaxRequest)
            {
                ClientScriptProxy.Current.RegisterJsResource(writer,
                    "Kiss.Web.Ajax.js");

                ClientScriptProxy.Current.RegisterJsResource(writer,
                    "Kiss.Web.jQuery.json.js");
            }

            StringBuilder sb = new StringBuilder();

            foreach (AjaxClass c in Find())
            {
                if (c.Id == "gAjax") // make sure gAjax script render only once in one http request
                {
                    if (!HttpContext.Current.Items.Contains(GAJAX))
                    {
                        HttpContext.Current.Items[GAJAX] = true;
                        AppendClassScript(c, ref  sb);
                    }
                }
                else
                    AppendClassScript(c, ref sb);
            }

            ClientScriptProxy.Current.RegisterJsBlock(writer,
                   Guid.NewGuid().ToString(),
                   sb.ToString(),
                   true,
                   jc.IsAjaxRequest);
        }

        #region helper

        private static void AppendClassScript(AjaxClass c, ref StringBuilder script)
        {
            script.AppendFormat("function {0}Class()", c.Id);
            script.Append("{};");
            script.AppendFormat("{0}Class.prototype=", c.Id);
            script.Append("{");

            for (int i = 0; i < c.Methods.Count; i++)
            {
                if (i > 0)
                    script.Append(",");

                AppendMethodScript(c.Id, c.Methods[i], ref script);
            }

            script.Append("};");
            script.AppendFormat("var {0}=new {0}Class();", c.Id);
        }

        private static void AppendMethodScript(string className, AjaxMethod m, ref StringBuilder script)
        {
            string ps = GetParamsScript(m.Params);

            if (m.Params.Count > 0)
                script.AppendFormat("{0}:function({1},cb)", m.MethodName, ps);
            else
                script.AppendFormat("{0}:function(cb)", m.MethodName);

            script.Append("{");
            script.AppendFormat("var args=[{0}]; ", ps);
            script.AppendFormat("return __ajaxManager.getData('{0}','{1}',args,'{2}',cb,'{3}');",
                className,
                m.MethodName,
                m.AjaxType,
                StringUtil.CombinUrl(JContext.Current.Area.VirtualPath, "ajax.aspx"));
            script.Append("}");
        }

        private static string GetParamsScript(List<AjaxParam> list)
        {
            return StringUtil.CollectionToCommaDelimitedString(list);
        }

        private List<AjaxClass> Find()
        {
            List<AjaxClass> list = new List<AjaxClass>();

            AjaxConfiguration config = AjaxConfiguration.GetConfig();
            foreach (string className in _methods.Keys)
            {
                AjaxClass c = config.FindClass(className);

                List<string> ms = _methods[className];
                if (ms.Count > 0)
                {
                    AjaxClass newc = c.Clone() as AjaxClass;

                    foreach (string methodName in _methods[className])
                    {
                        AjaxMethod m = config.FindMethod(c, methodName);//.Clone ( ) as AjaxMethod;
                        newc.Methods.Add(m);
                    }

                    c = newc;
                }

                list.Add(c);
            }


            if (JContext.Current.Controller != null)
            {
                Type t = JContext.Current.Controller.GetType();

                AjaxClass ac = null;
                if (AjaxConfiguration.ControllerAjax.ContainsKey(t))
                    ac = AjaxConfiguration.ControllerAjax[t];

                if (ac != null)
                {
                    if (StringUtil.IsNullOrEmpty(gAjaxMethods))
                        list.Add(ac);
                    else
                    {
                        List<string> methods = new List<string>(StringUtil.Split(gAjaxMethods, ",", true, true));

                        AjaxClass tmp = ac.Clone() as AjaxClass;
                        tmp.Methods = ac.Methods.FindAll(delegate(AjaxMethod am)
                        {
                            // 通配符（*）查找
                            bool match = false;
                            foreach (var item in methods)
                            {
                                if (item.StartsWith("*") && item.EndsWith("*"))
                                    match = am.MethodName.Contains(item.Substring(1, item.Length - 1));
                                else if (item.StartsWith("*"))
                                    match = am.MethodName.EndsWith(item.Substring(1), StringComparison.InvariantCultureIgnoreCase);
                                else if (item.EndsWith("*"))
                                    match = am.MethodName.StartsWith(item.Substring(0, item.Length - 1), StringComparison.InvariantCultureIgnoreCase);
                                else
                                    match = am.MethodName.Equals(item, StringComparison.InvariantCultureIgnoreCase);

                                if (match)
                                    return true;
                            }

                            return false;
                        });

                        list.Add(tmp);
                    }
                }
            }

            return list;
        }

        #endregion

        #region Method

        public static string GetScript(string className, string methodName)
        {
            AjaxConfiguration config = AjaxConfiguration.GetConfig();
            AjaxClass c_old = config.FindClass(className);
            AjaxMethod m = config.FindMethod(c_old, methodName);

            if (m == null)
                return string.Empty;

            AjaxClass c = c_old.Clone() as AjaxClass;
            c.Methods.Add(m);

            StringBuilder script = new StringBuilder();

            AppendClassScript(c, ref script);

            return script.ToString();
        }

        public static string GetScript(string className)
        {
            AjaxConfiguration config = AjaxConfiguration.GetConfig();
            AjaxClass c = config.FindClass(className);
            if (c == null)
                return string.Empty;

            StringBuilder script = new StringBuilder();

            AppendClassScript(c, ref script);

            return script.ToString();
        }

        #endregion
    }

    public class AjaxServerMethod : Control
    {
        private string _methodName;
        public string MethodName
        {
            get { return _methodName; }
            set { _methodName = value; }
        }
    }

    [ParseChildren(typeof(AjaxServerMethod)),
    PersistChildren(false)]
    public class AjaxServerClass : Control
    {
        private string _className;
        public string ClassName
        {
            get { return _className; }
            set { _className = value; }
        }

        private List<string> _methods = new List<string>();
        [Browsable(false)]
        public List<string> Methods
        {
            get { return _methods; }
        }

        protected override void AddParsedSubObject(object obj)
        {
            if (obj is AjaxServerMethod)
            {
                _methods.Add(((AjaxServerMethod)obj).MethodName);
            }
            else
            {
                base.AddParsedSubObject(obj);
            }
        }
    }
}