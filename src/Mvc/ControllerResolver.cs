using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Kiss.Utils;
using Kiss.Web.Ajax;
using Kiss.Web.UrlMapping;

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// use this class to resolve mvc controller
    /// </summary>
    public class ControllerResolver
    {
        #region fields

        protected Dictionary<string, Dictionary<string, Type>> controllerTypes = new Dictionary<string, Dictionary<string, Type>>();

        protected static readonly Type controllerBaseType = typeof(Controller);

        #endregion

        public static ControllerResolver Instance
        {
            get
            {
                if (Singleton<ControllerResolver>.Instance == null)
                {
                    Singleton<ControllerResolver>.Instance = new ControllerResolver();
                    Singleton<ControllerResolver>.Instance.Init();
                }

                return Singleton<ControllerResolver>.Instance;
            }
        }

        private ControllerResolver()
        {
        }

        public void Init()
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            this.ControllersResolved += resolver_ControllersResolved;

            string binPath = ServerUtil.MapPath("~/bin");

            foreach (Assembly asm in ServiceLocator.Instance.Resolve<ITypeFinder>().GetAssemblies())
            {
                if (asm.GetCustomAttributes(typeof(MvcAttribute), false).Length == 0)
                    continue;

                // only load assembly in bin dir
                if (!Directory.GetParent(new Uri(asm.CodeBase).LocalPath).FullName.Equals(binPath))
                    continue;

                foreach (var item in GetsControllerFromAssembly(asm))
                {
                    if (types.ContainsKey(item.Key))
                    {
                        LogManager.GetLogger<ControllerResolver>().Warn("controll's key conflicted! key:{0}, types:{1} and {2}. use first type as controller.",
                            item.Key,
                            types[item.Key].FullName,
                            item.Value.FullName);

                        continue;
                    }
                    types[item.Key] = item.Value;
                }
            }

            SetSiteControllers(SiteConfig.Instance.SiteKey, types);
        }

        /// <summary>
        /// set site's mvc controller types
        /// </summary>
        /// <param name="siteKey"></param>
        /// <param name="controllers"></param>
        public void SetSiteControllers(string siteKey, Dictionary<string, Type> controllers)
        {
            controllerTypes[siteKey] = controllers;

            ControllersResolvedEventArgs e = new ControllersResolvedEventArgs();
            e.ControllerTypes = controllers;
            e.SiteKey = siteKey;

            OnControllersResolved(e);
        }

        /// <summary>
        /// resolve controller types from an assembly
        /// </summary>
        /// <param name="asm"></param>
        /// <returns></returns>
        public Dictionary<string, Type> GetsControllerFromAssembly(Assembly asm)
        {
            Dictionary<string, Type> di = new Dictionary<string, Type>();
            if (asm.GetCustomAttributes(typeof(MvcAttribute), false).Length == 0)
                return di;

            Type[] ts = null;
            try
            {
                ts = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                //throw new MvcException(string.Format("get types of assembly: {0} failed.", asm.FullName), ex);
                ts = ex.Types;
            }

            if (ts == null || ts.Length == 0) return di;

            foreach (Type type in ts)
            {
                if (type == null || type.IsAbstract || type.IsInterface || type.IsGenericType || !type.IsSubclassOf(controllerBaseType))
                    continue;

                object[] objs = type.GetCustomAttributes(typeof(ControllerAttribute), true);

                if (objs.Length == 0)
                {
                    di[type.Name.Replace("Controller", string.Empty).ToLowerInvariant()] = type;
                }
                else
                {
                    foreach (ControllerAttribute attr in objs)
                    {
                        if (StringUtil.IsNullOrEmpty(attr.UrlId))
                            continue;

                        di[attr.UrlId.ToLowerInvariant()] = type;
                    }
                }
            }

            return di;
        }

        /// <summary>
        /// instance a mvc controller based on controller id
        /// </summary>
        /// <param name="controllerId"></param>
        /// <returns></returns>
        public Controller CreateController(string key)
        {
            Type t = GetControllerType(key);

            if (t == null) return null;

            Controller controller = Activator.CreateInstance(t) as Controller;
            if (controller == null)
                throw new MvcException("create mvc controller failed! key:{0}", key);

            return controller;
        }

        /// <summary>
        /// get mvc controller type of controller id
        /// </summary>
        /// <param name="controllerId"></param>
        /// <returns></returns>
        public Type GetControllerType(string key)
        {
            if (StringUtil.IsNullOrEmpty(key))
                return null;

            key = key.ToLowerInvariant();

            string sitekey = JContext.Current.Site.SiteKey;

            if (controllerTypes.ContainsKey(sitekey))
            {
                Dictionary<string, Type> types = controllerTypes[sitekey];
                if (types.ContainsKey(key))
                    return types[key];
                else
                {
                    // get controller from root site
                    Dictionary<string, Type> defaultControllers = null;
                    if (controllerTypes.ContainsKey(SiteConfig.Instance.SiteKey))
                        defaultControllers = controllerTypes[SiteConfig.Instance.SiteKey];

                    if (defaultControllers != null && defaultControllers.ContainsKey(key))
                        return defaultControllers[key];
                }
            }

            ControllerNotFoundEventArgs e = new ControllerNotFoundEventArgs() { ControllerId = key };
            OnControllerNotFound(e);
            return e.ControllerType;
        }

        #region event

        public event EventHandler<ControllersResolvedEventArgs> ControllersResolved;

        protected virtual void OnControllersResolved(ControllersResolvedEventArgs e)
        {
            EventHandler<ControllersResolvedEventArgs> handler = ControllersResolved;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<ControllerNotFoundEventArgs> ControllerNotFound;

        protected virtual void OnControllerNotFound(ControllerNotFoundEventArgs e)
        {
            EventHandler<ControllerNotFoundEventArgs> handler = ControllerNotFound;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        void resolver_ControllersResolved(object sender, ControllersResolvedEventArgs e)
        {
            logControllers(e.SiteKey, e.ControllerTypes);

            findAjaxMethods(e.ControllerTypes.Values);

            findRoutes(e.SiteKey, e.ControllerTypes);
        }

        public void findAjaxMethods(IEnumerable<Type> types)
        {
            foreach (Type t in types)
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

        void logControllers(string siteKey, Dictionary<string, Type> types)
        {
            StringBuilder mvclog = new StringBuilder();

            mvclog.AppendFormat("find {0} mvc controller in site:{1}.", types.Count, siteKey);
            mvclog.AppendLine();

            foreach (var item in types)
            {
                mvclog.AppendFormat("key:{0}-type:{1}", item.Key, item.Value.FullName);
                mvclog.AppendLine();
            }

            LogManager.GetLogger<ControllerResolver>().Debug(mvclog.ToString());
        }

        void findRoutes(string siteKey, Dictionary<string, Type> types)
        {
            UrlMappingConfig config = UrlMappingConfig.Instance;
            IUrlMappingProvider provider = ServiceLocator.Instance.Resolve<IUrlMappingProvider>();

            foreach (var controller in types)
            {
                foreach (MethodInfo m in controller.Value.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    object[] objs = m.GetCustomAttributes(typeof(UrlRouteAttribute), true);
                    if (objs.Length == 0)
                        continue;

                    UrlRouteAttribute attr = objs[0] as UrlRouteAttribute;

                    UrlMappingItem item = UrlMapping.Utility.CreateTemplatedMappingItem(string.Empty,
                        attr.Template,
                        UrlMapping.Utility.GetHref(attr.Href),
                        config.IncomingQueryStringBehavior);

                    item.UrlTemplate = attr.Template;

                    item.Index = -1;
                    item.SubIndex = -1;
                    item.Title = attr.Title;

                    item.Id = controller.Key;
                    item.Action = m.Name;

                    provider.AddMapping(siteKey, item);
                }
            }
        }
    }

    public class ControllersResolvedEventArgs : EventArgs
    {
        public static readonly new ControllersResolvedEventArgs Empty = new ControllersResolvedEventArgs();

        public Dictionary<string, Type> ControllerTypes { get; set; }
        public string SiteKey { get; set; }
    }

    public class ControllerNotFoundEventArgs : EventArgs
    {
        public string ControllerId { get; set; }
        public Type ControllerType { get; set; }
    }
}
