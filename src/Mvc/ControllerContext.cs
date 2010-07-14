using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Kiss.Utils;
using Kiss.Web.Ajax;

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// mvc controller container
    /// </summary>
    public class ControllerContainer
    {
        internal Dictionary<string, Type> controllerTypes = new Dictionary<string, Type>();

        static ILogger logger;

        public void Init()
        {
            logger = LogManager.GetLogger<ControllerContainer>();

            Type controllerBaseType = typeof(Controller);

            foreach (Assembly asm in ServiceLocator.Instance.Resolve<ITypeFinder>().GetAssemblies())
            {
                if (asm.GetCustomAttributes(typeof(MvcAttribute), false).Length == 0)
                    continue;

                Type[] ts = null;
                try
                {
                    ts = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    throw new MvcException(string.Format("get types of assembly: {0} failed.", asm.FullName), ex);
                }

                if (ts == null || ts.Length == 0) continue;

                foreach (Type type in ts)
                {
                    if (type.IsAbstract || type.IsInterface || !type.IsSubclassOf(controllerBaseType))
                        continue;

                    object[] objs = type.GetCustomAttributes(typeof(ControllerAttribute), true);

                    if (objs.Length == 0)
                    {
                        controllerTypes[type.Name.Replace("Controller", string.Empty).ToLowerInvariant()] = type;
                    }
                    else
                    {
                        foreach (ControllerAttribute attr in objs)
                        {
                            if (StringUtil.IsNullOrEmpty(attr.UrlId))
                                continue;

                            controllerTypes[attr.UrlId.ToLowerInvariant()] = type;
                        }
                    }
                }
            }

            logControllers();

            // find ajax class
            findAjaxMethods();
        }

        private void findAjaxMethods()
        {
            foreach (Type t in controllerTypes.Values)
            {
                AjaxClass ac = new AjaxClass();

                ac.Id = "gAjax";
                ac.Type = t;

                foreach (MethodInfo mi in t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    object[] ajaxattrs = mi.GetCustomAttributes(typeof(AjaxMethodAttribute), true);
                    if (ajaxattrs != null && ajaxattrs.Length == 1)
                    {
                        AjaxMethodAttribute am = ajaxattrs[0] as AjaxMethodAttribute;
                        AjaxMethod ajaxMethod = new AjaxMethod();
                        ajaxMethod.MethodName = mi.Name;
                        ajaxMethod.AjaxType = am.Type.ToString();
                        ajaxMethod.CacheMinutes = am.CacheMinutes;
                        ajaxMethod.Exception = new AjaxServerException() { Action = am.OnExceptionAction, Parameter = am.OnExceptionParameter };

                        foreach (ParameterInfo param in mi.GetParameters())
                        {
                            string paramType = string.Empty;
                            if (param.ParameterType == typeof(long))
                                paramType = "long";
                            else if (param.ParameterType == typeof(int))
                                paramType = "int";
                            else if (param.ParameterType == typeof(double))
                                paramType = "double";
                            else if (param.ParameterType == typeof(bool))
                                paramType = "bool";
                            else if (param.ParameterType == typeof(string[]))
                                paramType = "strings";
                            else if (param.ParameterType == typeof(int[]))
                                paramType = "ints";
                            else
                                paramType = param.ParameterType.Name;

                            ajaxMethod.Params.Add(new AjaxParam() { ParamType = paramType, ParamName = param.Name });
                        }

                        ac.Methods.Add(ajaxMethod);
                    }
                }

                AjaxConfiguration.ControllerAjax[t] = ac;
            }
        }

        private void logControllers()
        {
            StringBuilder mvclog = new StringBuilder();

            mvclog.AppendFormat("find {0} mvc controller.", controllerTypes.Count);
            mvclog.AppendLine();

            foreach (var item in controllerTypes)
            {
                mvclog.AppendFormat("key:{0}    type:{1}", item.Key, item.Value.FullName);
                mvclog.AppendLine();
            }

            logger.Debug(mvclog.ToString());
        }

        public Controller CreateController(string key)
        {
            Type t = GetControllerType(key);
            if (t == null)
                return null;

            Controller controller = Activator.CreateInstance(t) as Controller;
            if (controller == null)
                throw new MvcException("create mvc controller failed! key:{0}", key);

            return controller;
        }

        public Type GetControllerType(string key)
        {
            if (StringUtil.IsNullOrEmpty(key))
                return null;

            key = key.ToLowerInvariant();

            if (controllerTypes.ContainsKey(key))
                return controllerTypes[key];

            return null;
        }
    }
}
