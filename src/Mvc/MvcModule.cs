﻿using System;
using System.Threading;
using System.Web;

namespace Kiss.Web.Mvc
{
    public class MvcModule
    {
        internal ActionInvoker invoker;

        protected virtual void Invoke(object sender, EventArgs e)
        {
            if (EventBroker.IsStaticResource((sender as HttpApplication).Request))
                return;

            JContext jc = JContext.Current;

            try
            {
                jc.Controller = ControllerResolver.Instance.CreateController(jc.Navigation.Id);
                if (jc.Controller == null)
                    return;

                jc.Controller.jc = jc;
                jc.ViewData["this"] = jc.Controller;

                jc.IsAsync = invoker.IsAsync(jc);

                if (!jc.IsAsync)
                    invoker.InvokeAction(jc);
            }
            catch (ThreadAbortException) { }// ignore this exception            
        }

        public virtual void Start()
        {
            invoker = new ActionInvoker();

            //EventBroker.Instance.PostAcquireRequestState += Invoke;
            EventBroker.Instance.PreRequestHandlerExecute += Invoke;
        }
    }
}
