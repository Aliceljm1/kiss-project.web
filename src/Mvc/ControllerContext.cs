#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-09-24
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-09-24		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using Kiss.Query;
using Kiss.Utils;
using Kiss.Web.Ajax;

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// context based on url
    /// </summary>
    public class ControllerContext : IControllerContext
    {
        internal Dictionary<string, Type> controllerTypes = new Dictionary<string, Type>();
        private Dictionary<string, Type> serviceTypes = new Dictionary<string, Type>();
        private Dictionary<string, Type> qcTypes = new Dictionary<string, Type>();
        private Dictionary<string, Type> modelTypes = new Dictionary<string, Type>();

        static ILogger logger;

        public void Init()
        {
            logger = LogManager.GetLogger<ControllerContext>();

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

                Type modelBaseType = typeof(Obj);
                Type qcBaseType = typeof(QueryCondition);
                Type controllerBaseType = typeof(Controller);

                List<Type> ms = new List<Type>();

                foreach (Type type in ts)
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    // save all model type
                    if (type.IsSubclassOf(modelBaseType))
                        ms.Add(type);

                    object[] objs = type.GetCustomAttributes(typeof(ControllerAttribute), true);

                    if (objs.Length == 0 && type.Name.EndsWith("Controller", StringComparison.InvariantCultureIgnoreCase))
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

                            if (attr.ModelType != null && attr.ModelType.IsSubclassOf(modelBaseType))
                                modelTypes[attr.UrlId.ToLowerInvariant()] = attr.ModelType;

                            if (attr.QcType != null && attr.QcType.IsSubclassOf(qcBaseType))
                                qcTypes[attr.UrlId.ToLowerInvariant()] = attr.QcType;

                            if (attr.ServiceType != null)
                                serviceTypes[attr.UrlId.ToLowerInvariant()] = attr.ServiceType;
                        }
                    }
                }

                // remove key if key is manual configed
                ms.RemoveAll(delegate(Type t)
                {
                    return modelTypes.ContainsKey(t.Name.ToLowerInvariant());
                });

                foreach (Type modelType in ms)
                {
                    string key = modelType.Name.ToLowerInvariant();
                    modelTypes[key] = modelType;

                    foreach (Type t in ts)
                    {
                        if (!t.Name.StartsWith(modelType.Name, StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        if (t.IsSubclassOf(qcBaseType) && t.Name.Equals(key + "Query", StringComparison.InvariantCultureIgnoreCase))
                            qcTypes[key] = t;
                        else if (t.Name.Equals(key + "Controller", StringComparison.InvariantCultureIgnoreCase))
                            controllerTypes[key] = t;
                        else if (t.Name.Equals(key + "Manager", StringComparison.InvariantCultureIgnoreCase))
                            serviceTypes[key] = t;
                    }
                }
            }

            logger.Debug("find {0} mvc controller.", controllerTypes.Count);

            // find ajax class
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

        public Type CurrentModelType { get { return GetModelType(ControllerName); } }

        public object CurrentController { get { return CreateController(ControllerName); } }

        public QueryCondition CurrentQc { get { return CreateQC(ControllerName); } }

        public object CreateController(string key)
        {
            Type t = GetControllerType(key);
            if (t == null)
                return null;

            return Activator.CreateInstance(t);
        }

        public object CreateController(Type modelType)
        {
            string key = string.Empty;
            foreach (KeyValuePair<string, Type> pair in modelTypes)
            {
                if (pair.Value == modelType)
                {
                    key = pair.Key;
                    break;
                }
            }

            return CreateController(key);
        }

        public QueryCondition CreateQC(string key)
        {
            if (StringUtil.IsNullOrEmpty(key))
                return null;

            key = key.ToLowerInvariant();

            if (qcTypes.ContainsKey(key))
            {
                Type queryType = qcTypes[key];

                return Activator.CreateInstance(queryType) as QueryCondition;
            }

            return null;
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

        public Type GetModelType(string key)
        {
            if (StringUtil.IsNullOrEmpty(key))
                return null;

            key = key.ToLowerInvariant();

            if (modelTypes.ContainsKey(key))
                return modelTypes[key];

            return null;
        }

        public string ActionName
        {
            get { return JContext.Current.Navigation.Action; }
        }

        public string ControllerName
        {
            get { return JContext.Current.Navigation.Id; }
        }

        public Type CurrentControllerType { get { return GetControllerType(ControllerName); } }

        public IRepository CurrentService { get { return CreateService(ControllerName); } }

        public IRepository CreateService(string key)
        {
            Type t = GetServiceType(key);
            if (t == null)
                return null;

            return QueryObject.GetRepository(t);
        }

        public IRepository CreateService(Type modelType)
        {
            string key = string.Empty;
            foreach (KeyValuePair<string, Type> pair in modelTypes)
            {
                if (pair.Value == modelType)
                {
                    key = pair.Key;
                    break;
                }
            }

            return CreateService(key);
        }

        private Type GetServiceType(string key)
        {
            if (StringUtil.IsNullOrEmpty(key))
                return null;

            key = key.ToLowerInvariant();

            if (serviceTypes.ContainsKey(key))
                return serviceTypes[key];

            return null;
        }
    }
}
