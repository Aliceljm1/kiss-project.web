#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-10-10
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-10-10		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System;
using System.Collections.Generic;

namespace Kiss.Web.Mvc
{
    public class MvcModule : IStartable
    {
        private ActionInvoker invoker;

        public ControllerContext Context { get; private set; }

        protected virtual void Invoke(object sender, EventArgs e)
        {
            invoker.InvokeAction(Context);
        }

        public virtual void Start()
        {
            invoker = new ActionInvoker();

            Context = new ControllerContext();
            Context.Init();

            ControllersResolvedEventArgs e = new ControllersResolvedEventArgs();
            e.ControllerTypes = Context.controllerTypes;

            OnControllersResolved(e);

            EventBroker.Instance.PostMapRequestHandler += Invoke;
        }

        #region event

        public class ControllersResolvedEventArgs : EventArgs
        {
            public static readonly new ControllersResolvedEventArgs Empty = new ControllersResolvedEventArgs();
            public Dictionary<string, Type> ControllerTypes { get; set; }
        }

        public static event EventHandler<ControllersResolvedEventArgs> ControllersResolved;

        protected virtual void OnControllersResolved(ControllersResolvedEventArgs e)
        {
            EventHandler<ControllersResolvedEventArgs> handler = ControllersResolved;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
    }
}
