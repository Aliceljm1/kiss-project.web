using System;
using System.Collections.Generic;

namespace Kiss.Web.Mvc
{
    public class MvcModule : IStartable
    {
        internal ActionInvoker invoker;

        public ControllerContainer Container { get; private set; }

        protected virtual void Invoke(object sender, EventArgs e)
        {
            JContext jc = JContext.Current;
            
            jc.Controller = Container.CreateController(jc.Navigation.Id);
            jc.Controller.jc = jc;

            jc.IsAsync = invoker.IsAsync(jc);

            if (!jc.IsAsync)
                invoker.InvokeAction(jc);
        }

        public virtual void Start()
        {
            invoker = new ActionInvoker();

            Container = new ControllerContainer();
            Container.Init();

            ControllersResolvedEventArgs e = new ControllersResolvedEventArgs();
            e.ControllerTypes = Container.controllerTypes;

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
