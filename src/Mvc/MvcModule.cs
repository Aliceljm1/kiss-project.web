using System;
using System.Collections.Generic;
using Kiss.Utils;
using System.Threading;

namespace Kiss.Web.Mvc
{
    public class MvcModule : IStartable
    {
        static readonly ILogger logger = LogManager.GetLogger<MvcModule>();

        internal ActionInvoker invoker;

        public ControllerContainer Container { get; private set; }

        protected virtual void Invoke(object sender, EventArgs e)
        {
            JContext jc = JContext.Current;

            try
            {
                jc.Controller = Container.CreateController(jc.Navigation.Id);
                if (jc.Controller == null)
                    return;

                jc.Controller.jc = jc;
                jc.ViewData["this"] = jc.Controller;

                jc.IsAsync = invoker.IsAsync(jc);

                if (!jc.IsAsync)
                    invoker.InvokeAction(jc);
            }
            catch (ThreadAbortException) { }// ignore this exception
            catch (Exception ex)
            {
                logger.Fatal(ExceptionUtil.WriteException(ex));

                if (jc.Context.IsDebuggingEnabled)
                    throw ex;
            }
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
