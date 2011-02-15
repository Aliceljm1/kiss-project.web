#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-09-11
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-09-08		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Kiss.Utils;
using Newtonsoft.Json;

namespace Kiss.Web.Ajax
{
    /// <summary>
    /// ajax method
    /// </summary>
    public class AjaxMethod
    {
        #region fields / props

        private static readonly ILogger logger = LogManager.GetLogger<AjaxMethod>();

        // Get/Post
        public string AjaxType { get; set; }

        private string _methodName;
        public string MethodName
        {
            get { return _methodName; }
            set { _methodName = value; }
        }

        private List<AjaxParam> _params = new List<AjaxParam>();
        public List<AjaxParam> Params
        {
            get { return _params; }
            set { _params = value; }
        }

        private AjaxServerException _exception;
        public AjaxServerException Exception
        {
            get { return _exception; }
            set { _exception = value; }
        }

        /// <summary>
        /// default cache 1 hour.
        /// </summary>
        public int CacheMinutes { get; set; }

        /// <summary>
        /// a string used to test if cache is enabled
        /// </summary>
        public string CacheTest { get; set; }

        private MethodInfo _methodInfo;

        #endregion

        #region ctor

        public AjaxMethod()
        {
            AjaxType = "Get";
        }

        #endregion

        #region methods

        public object Invoke(Type type, string argsJson)
        {
            object[] args = ConstructMethodArgs(_params, argsJson);
            Type[] argsType = constructMethodTypes(args);

            if (_methodInfo == null)
                _methodInfo = type.GetMethod(_methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance, null, argsType, null);

            if (_methodInfo == null)
            {
                // log
                logger.Error("ajax method: {0} not found.", _methodName);
                return null;
            }

            if (_methodInfo.IsStatic)
                return _methodInfo.Invoke(null, args);
            else
                return _methodInfo.Invoke(Activator.CreateInstance(type), args);
        }

        public object Invoke(string type, string argsJson)
        {
            Type t = TypeRegistry.ResolveType(type);

            return Invoke(t, argsJson);
        }

        #endregion

        #region helper

        private static Type[] constructMethodTypes(object[] methodArgs)
        {
            try
            {
                if (methodArgs == null)
                    return new Type[] { };
                Type[] argsType = new Type[methodArgs.Length];
                // check for method casting - dynamic cast if necessary
                for (int i = 0; i < methodArgs.Length; i++)
                {
                    argsType[i] = methodArgs[i].GetType();
                }
                return argsType;
            }
            catch (Exception ex)
            {
                logger.Error("constructMethodTypes failed.", ex);
                return null;
            }
        }

        private static object[] ConstructMethodArgs(List<AjaxParam> ps, string methodJsonArgs)
        {
            ArrayList argsObject;

            try
            {
                if (string.IsNullOrEmpty(methodJsonArgs))
                    argsObject = new ArrayList() { string.Empty };
                else
                    argsObject = JavaScriptConvert.DeserializeObject<ArrayList>(methodJsonArgs);

                for (int i = 0; i < ps.Count; i++)
                {
                    AjaxParam p = ps[i];
                    string pType = p.ParamType.ToLower();
                    object data = argsObject[i];

                    switch (pType)
                    {
                        case "long":
                            argsObject[i] = Convert.ToInt64(data);
                            break;
                        case "int":
                            argsObject[i] = Convert.ToInt32(data);
                            break;
                        case "double":
                            argsObject[i] = Convert.ToDouble(data);
                            break;
                        case "bool":
                            argsObject[i] = Convert.ToBoolean(data);
                            break;
                        case "string":
                            argsObject[i] = data == null ? string.Empty : data.ToString();
                            break;
                        case "strings":
                            if (data is IList)
                            {
                                List<string> list = new List<string>();
                                foreach (object item in (IList)data)
                                {
                                    list.Add(item.ToString());
                                }
                                argsObject[i] = list.ToArray();
                            }
                            break;
                        case "ints":
                            if (data is IList)
                            {
                                List<int> list = new List<int>();
                                foreach (object item in (IList)data)
                                {
                                    list.Add(Convert.ToInt32(item));
                                }
                                argsObject[i] = list.ToArray();
                            }
                            break;
                        default:
                            break;
                    }
                }

                return argsObject.ToArray();
            }
            catch (Exception ex)
            {
                logger.Error("ConstructMethodArgs failed", ex);
                return null;
            }
        }

        #endregion
    }
}
