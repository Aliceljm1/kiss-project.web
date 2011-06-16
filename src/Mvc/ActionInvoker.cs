using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Kiss.Json;
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

                // before execute action
                Controller.BeforeActionExecuteEventArgs e = new Controller.BeforeActionExecuteEventArgs(jc);
                jc.Controller.OnBeforeActionExecute(e);
                if (e.PreventDefault)
                {
                    ret = e.ReturnValue;
                }

                if (jc.IsPost)
                {
                    // set rendercontent to false before invoke action
                    jc.RenderContent = false;

                    if (!e.PreventDefault)
                    {
                        if (mi.GetParameters().Length == 1)
                            ret = mi.Invoke(jc.Controller, new object[] { jc.Context.Request.Form });
                        else
                            ret = mi.Invoke(jc.Controller, null);
                    }

                    if (ret != null && !jc.RenderContent)
                        jc.Context.Response.Write(new JavaScriptSerializer().Serialize(ret));
                }
                else
                {
                    if (!e.PreventDefault)
                    {
                        ret = mi.Invoke(jc.Controller, null);
                    }

                    if (ret != null && ret is ActionResult)
                    {
                        ActionResult actionResult = ret as ActionResult;
                        actionResult.ExecuteResult(jc);
                    }
                }

                // after execute action
                jc.Controller.OnAfterActionExecute(ret);
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

            string action = jc.Navigation.Action + ":" + jc.IsPost;

            if (mis.ContainsKey(action))
                mi = mis[action];
            else
            {
                List<MethodInfo> methods = new List<MethodInfo>(t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
                methods.Sort((x, y) =>
                {
                    bool hasPostAttr = x.GetCustomAttributes(typeof(HttpPostAttribute), false).Length == 1;
                    bool hasPostAttr_2 = y.GetCustomAttributes(typeof(HttpPostAttribute), false).Length == 1;

                    return hasPostAttr_2.CompareTo(hasPostAttr);
                });

                foreach (MethodInfo m in methods)
                {
                    bool hasPostAttr = m.GetCustomAttributes(typeof(HttpPostAttribute), false).Length == 1;
                    bool hasAjaxAttr = m.GetCustomAttributes(typeof(Ajax.AjaxMethodAttribute), true).Length > 0;

                    if (!m.ContainsGenericParameters &&
                        m.Name.Equals(jc.Navigation.Action, StringComparison.InvariantCultureIgnoreCase) &&
                         !hasAjaxAttr &&
                        ((jc.IsPost && hasPostAttr) || !hasPostAttr) &&
                        ((jc.IsPost && m.GetParameters().Length <= 1) || (!jc.IsPost && m.GetParameters().Length == 0))
                        )
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
