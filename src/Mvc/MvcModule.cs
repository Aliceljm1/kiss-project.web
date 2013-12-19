using Kiss.Web.Controls;
using Kiss.Web.Utils;
using System;
using System.Threading;
using System.Web;
using Kiss.Utils;

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
                {
                    if (jc.IsEmbed)
                    {
                        jc.RenderContent = false;

                        ResponseUtil.OutputJson(jc.Context.Response,
                            new TemplatedControl() { UsedInMvc = jc.Context.Request.Headers["usedinmvc"].ToBoolean(true), OverrideSkinName = true, Templated = true }.Execute());
                    }
                    return;
                }

                object[] attrs = jc.Controller.GetType().GetCustomAttributes(typeof(CheckLicenceAttribute), true);
                if (attrs.Length == 1)
                {
                    ILicenceProvider lp = ServiceLocator.Instance.SafeResolve<ILicenceProvider>();

                    if (lp != null && !lp.Check())
                    {
                        if (!lp.OnLicenceInvalid())
                            return;
                    }
                }

                jc.Controller.jc = jc;
                jc.ViewData["this"] = jc.Controller;

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
