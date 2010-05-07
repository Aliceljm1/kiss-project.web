#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-10-10
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-10-10		zhli Comment Created
//+-------------------------------------------------------------------+
//+ 2009-10-13		zhli allow private method
//+-------------------------------------------------------------------+
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// mvc controller action invoker
    /// </summary>
    public class ActionInvoker : IActionInvoker
    {
        private readonly SingleEntryGate _executeWasCalledGate = new SingleEntryGate();
        private Dictionary<Type, Dictionary<string, MethodInfo>> _mis = new Dictionary<Type, Dictionary<string, MethodInfo>>();

        #region IActionInvoker Members

        public bool InvokeAction(IControllerContext controllerContext)
        {
            object controller = controllerContext.CurrentController;
            if (controller == null)
                return false;

            // save to jcontext
            JContext.Current.Controller = controller;

            Type t = controller.GetType();

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

            if (mis.ContainsKey(controllerContext.ActionName))
                mi = mis[controllerContext.ActionName];
            else
            {
                foreach (MethodInfo m in t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (m.Name.Equals(controllerContext.ActionName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        mi = m;
                        mis[controllerContext.ActionName] = mi;
                        break;
                    }
                    mis[controllerContext.ActionName] = null;
                }
            }

            if (mi == null)
                return false;

            object ret;

            try
            {
                if (mi.IsStatic)
                    ret = mi.Invoke(null, null);
                else
                    ret = mi.Invoke(controller, null);
            }
            catch (TargetInvocationException ex)
            {
                throw new MvcException(string.Format("execute controller: {0}'s method: {1} failed. {2}",
                    controllerContext.ControllerName,
                    controllerContext.ActionName,
                    ex.InnerException.Message), ex.InnerException);
            }

            if (ret != null)
            {
                if (ret is ActionResult)
                {
                    ActionResult actionResult = ret as ActionResult;
                    actionResult.ExecuteResult(controllerContext);
                }
            }

            return true;
        }

        #endregion
    }
}
