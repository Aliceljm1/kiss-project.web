using System;
using System.Threading;
using Kiss.Utils;

namespace Kiss.Web.Mvc
{
    public class MvcModule
    {
        static readonly ILogger logger = LogManager.GetLogger<MvcModule>();

        internal ActionInvoker invoker;

        protected virtual void Invoke(object sender, EventArgs e)
        {
            JContext jc = JContext.Current;

            try
            {
                jc.Controller = ControllerResolver.Instance.CreateController(jc.Navigation.Id);
                if (jc.Controller == null)
                    return;

                logger.Debug("begin invoke controller's action. {0}", jc.Navigation.ToString());

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

            //EventBroker.Instance.PostAcquireRequestState += Invoke;
            EventBroker.Instance.PreRequestHandlerExecute += Invoke;
        }
    }
}
