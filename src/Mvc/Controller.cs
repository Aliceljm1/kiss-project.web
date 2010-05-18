using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Kiss.Query;

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// base controller
    /// </summary>
    public class Controller
    {
        public IRepository<T> GetRepository<T>() where T : IQueryObject
        {
            return QueryObject.GetRepository<T>();
        }

        public IRepository<T, t> GetRepository<T, t>() where T : Obj<t>
        {
            return QueryObject.GetRepository<T, t>();
        }

        protected Dictionary<string, object> ViewData { get { return jc.ViewData; } }

        private JContext _jc;
        public JContext jc { get { if (_jc == null)_jc = JContext.Current; return _jc; } set { _jc = value; } }

        protected HttpContext httpContext { get { return jc.Context; } }

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

        protected virtual void list<T>() where T : IQueryObject
        {
            QueryCondition q = new WebQuery();
            q.LoadCondidtion();

            list<T>(q);
        }

        protected virtual void list<T>(QueryCondition q) where T : IQueryObject
        {
            IRepository<T> repo = GetRepository<T>();

            q.TotalCount = repo.Count(q);

            ViewData["q"] = q;
            ViewData["list"] = repo.Gets(q);
        }

        public bool can(string permission)
        {
            return jc.User.HasPermission(permission);
        }
    }
}
