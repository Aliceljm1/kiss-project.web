using System.Collections.Specialized;
using Kiss.Query;
using Kiss.Utils;

namespace Kiss.Web
{
    /// <summary>
    /// query condition in http context
    /// </summary>
    public class WebQuery : QueryCondition
    {
        private JContext _jc;
        public JContext jc
        {
            get
            {
                if (_jc == null)
                    _jc = JContext.Current;
                return _jc;
            }
        }

        public NameValueCollection qc { get { return jc.QueryString; } }

        #region ctor

        public WebQuery()
        {
        }

        public WebQuery(string connstr_name)
            : base(connstr_name)
        {
        }

        #endregion

        /// <summary>
        /// 获取查询条件
        /// </summary>
        public override void LoadCondidtion()
        {
            base.LoadCondidtion();

            Keyword = string.IsNullOrEmpty(jc.QueryText) ? string.Empty : jc.QueryText.Replace("'", "''");

            PageIndex = jc.PageIndex;

            string orderby = jc.QueryString["sort"];

            foreach (string str in StringUtil.Split(orderby, "+", true, true))
            {
                AppendOrderby(str.TrimStart('-'), !str.StartsWith("-"));
            }
        }

        public override string this[string key]
        {
            get
            {
                string val = base[key];
                if (string.IsNullOrEmpty(val))// get from context
                {
                    if (jc.IsPost)
                        val = jc.Context.Request.Form[key];
                    else
                        val = jc.QueryString[key];

                    val = string.IsNullOrEmpty(val) ? string.Empty : val.Replace("'", "''");

                    base[key] = val;
                }

                return val;
            }
        }
    }
}
