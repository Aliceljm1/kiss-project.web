using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Kiss.Query;
using Kiss.Utils;
using Kiss.Web.Controls;

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// base mvc controller
    /// </summary>
    public class Controller
    {
        /// <summary>
        /// controller's view data
        /// </summary>
        protected Dictionary<string, object> ViewData { get { return jc.ViewData; } }

        private JContext _jc;
        /// <summary>
        /// jcontext
        /// </summary>
        public JContext jc { get { if (_jc == null)_jc = JContext.Current; return _jc; } set { _jc = value; } }

        protected HttpContext httpContext { get { return jc.Context; } }

        protected NameValueCollection querystring { get { return jc.QueryString; } }
        protected NameValueCollection form { get { return httpContext.Request.Form; } }

        private ILogger _logger;
        protected ILogger logger
        {
            get
            {
                if (_logger == null)
                    _logger = LogManager.GetLogger(GetType());
                return _logger;
            }
        }

        /// <summary>
        /// paging list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected void list<T>() where T : IQueryObject
        {
            list<T>(string.Empty);
        }

        /// <summary>
        /// paging list
        /// </summary>
        protected void list<T>(string queryId) where T : IQueryObject
        {
            QueryCondition q = new WebQuery();
            q.LoadCondidtion();

            q.Id = queryId;

            list<T>(q);
        }

        /// <summary>
        /// paging list
        /// </summary>
        protected void list<T>(QueryCondition q) where T : IQueryObject
        {
            IRepository<T> repo = QueryObject<T>.Repository;

            ViewData["list"] = repo.GetDataTable(q).Rows;

            if (q.Paging)
                q.TotalCount = repo.Count(q);

            ViewData["q"] = q;
        }

        /// <summary>
        /// get html to edit an object
        /// </summary>
        protected string edit<T, t>(t id, string skinName) where T : Obj<t>
        {
            ViewData["item"] = QueryObject<T, t>.Repository.Get(id);

            return new TemplatedControl(skinName).Execute();
        }

        /// <summary>
        /// save object(pk is int) from formdata
        /// </summary>
        protected T save<T>(string formdata) where T : Obj<int>
        {
            return save<T, int>(formdata);
        }

        /// <summary>
        /// save object(pk is string) from formdata
        /// </summary>
        protected T save2<T>(string formdata) where T : Obj<string>
        {
            return save<T, string>(formdata);
        }

        /// <summary>
        /// save object from formdata, using default convert util
        /// </summary>
        protected T save<T, t>(string formdata) where T : Obj<t>
        {
            return QueryObject<T, t>.Save(formdata, delegate(T obj, NameValueCollection nv)
            {
                TypeConvertUtil.ConvertFrom(obj, nv);

                return true;
            });
        }

        /// <summary>
        /// checking permission
        /// </summary>
        public bool can(string permission)
        {
            if (jc.User == null)
                return true;

            return jc.User.HasPermission(permission);
        }

        /// <summary>
        /// check if current user has permission to specified menu
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public bool hasMenu(string menu)
        {
            if (jc.User == null)
                return true;

            return jc.User.HasPermission(string.Concat("menu:", jc.Site.SiteKey, "_", menu));
        }

        protected string ContentType { get { return httpContext.Items["_ContentType_"] as string ?? httpContext.Response.ContentType; } set { httpContext.Items["_ContentType_"] = value; httpContext.Response.ContentType = value; } }
    }
}
