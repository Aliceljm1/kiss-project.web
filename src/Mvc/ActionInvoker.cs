using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Kiss.Security;
using Kiss.Utils;

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// mvc controller action invoker
    /// </summary>
    public class ActionInvoker : IActionInvoker
    {
        private readonly SingleEntryGate _executeWasCalledGate = new SingleEntryGate();
        private Dictionary<Type, Dictionary<string, MethodInfo>> _mis = new Dictionary<Type, Dictionary<string, MethodInfo>>();

        public bool IsAsync(JContext jc)
        {
            MethodInfo mi = getActionMethod(jc);
            if (mi == null)
                return false;

            return mi.GetCustomAttributes(typeof(AsyncAttribute), true).Length > 0;
        }

        public bool InvokeAction(JContext jc)
        {
            MethodInfo mi = getActionMethod(jc);

            if (mi == null)
                return false;

            object ret = null;

            try
            {
                if (jc.User != null)
                {
                    object[] attrs = mi.GetCustomAttributes(typeof(PermissionAttribute), true);
                    if (attrs.Length > 0)
                    {
                        PermissionAttribute attr = attrs[0] as PermissionAttribute;
                        if (!string.IsNullOrEmpty(attr.Permission))
                        {
                            if (jc.User.HasPermission(attr.Permission))
                                goto execute;
                            else
                                jc.User.OnPermissionDenied(new PermissionDeniedEventArgs(attr.Permission));
                        }
                    }
                }
                else
                {
                    goto execute;
                }

            execute:
                if (mi.IsStatic)
                    ret = mi.Invoke(null, null);
                else
                    ret = mi.Invoke(jc.Controller, null);
            }
            catch (ThreadAbortException) { }// ignore this exception
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                    ex = ex.InnerException;

                LogManager.GetLogger<ActionInvoker>().Error(ExceptionUtil.WriteException(ex));

                throw new MvcException(string.Format("execute controller: {0}'s method: {1} failed. {2}",
                    jc.Controller.GetType().Name,
                    mi.Name,
                    ex.Message), ex);
            }

            if (ret != null)
            {
                if (ret is ActionResult)
                {
                    ActionResult actionResult = ret as ActionResult;
                    actionResult.ExecuteResult(jc);
                }
            }

            return true;
        }

        private MethodInfo getActionMethod(JContext jc)
        {
            Type t = jc.Controller.GetType();

            Dictionary<string, MethodInfo> mis;

            if (_mis.ContainsKey(t))
            {
                mis = _mis[t];
            }
            else
            {
                mis = new Dictionary<string, MethodInfo>();
                _mis[t] = mis;
            }

            MethodInfo mi = null;

            string action = jc.Navigation.Action;

            if (mis.ContainsKey(action))
                mi = mis[action];
            else
            {
                foreach (MethodInfo m in t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (m.Name.Equals(action, StringComparison.InvariantCultureIgnoreCase))
                    {
                        mi = m;
                        mis[action] = mi;
                        break;
                    }
                    mis[action] = null;
                }
            }
            return mi;
        }
    }
}
