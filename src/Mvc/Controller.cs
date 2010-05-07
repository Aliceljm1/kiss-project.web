using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Kiss.Query;
using Kiss.Utils;

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// base controller
    /// </summary>
    public class Controller
    {
        private IControllerContext _context;
        protected IControllerContext context
        {
            get
            {
                if (_context == null)
                    _context = JContext.Current.ControllerContext;
                return _context;
            }
        }

        private IRepository _repository;
        public IRepository Repository
        {
            get
            {
                if (_repository == null)
                {
                    _repository = context.CurrentService;
                    if (_repository == null)
                        throw new MvcException("current service is not set.");
                }
                return _repository;
            }
        }

        public IRepository<T> GetRepository<T>() where T : IQueryObject
        {
            return QueryObject.GetRepository<T>();
        }

        public IRepository<T> GetRepository<T, t>() where T : Obj<t>
        {
            return QueryObject.GetRepository<T, t>();
        }

        private Dictionary<string, object> _viewData;
        protected Dictionary<string, object> ViewData
        {
            get
            {
                if (_viewData == null)
                    _viewData = JContext.Current.ViewData;
                return _viewData;
            }
        }

        private JContext _jc;
        protected JContext jc { get { if (_jc == null)_jc = JContext.Current; return _jc; } }

        protected HttpContext httpContext { get { return HttpContext.Current; } }

        protected NameValueCollection querystring { get { return jc.QueryString; } }
        protected NameValueCollection form { get { return httpContext.Request.Form; } }

        private ILogger _logger;
        protected virtual ILogger logger
        {
            get
            {
                if (_logger == null)
                    _logger = LogManager.GetLogger(GetType());
                return _logger;
            }
        }

        protected virtual void list()
        {
            //QueryCondition q = context.CurrentQc;
            //if (q == null)
            //{
            //    q = new QueryCondition();
            //    q.LoadCondidtion();

            //    q.TotalCount = Repository.Count();
            //    ViewData["list"] = DataUtil.Convent2Bindable(Repository.GetsPaged(q.PageIndex, q.PageSize));
            //}
            //else
            //{
            //    q.LoadCondidtion();
            //    q.TotalCount = Repository.Count(q);

            //    ViewData["list"] = DataUtil.Convent2Bindable(Repository.Gets(q));
            //}

            //ViewData["q"] = q;
        }

        protected bool Can(string permission)
        {
            return jc.User.HasPermission(permission);
        }
    }
}
