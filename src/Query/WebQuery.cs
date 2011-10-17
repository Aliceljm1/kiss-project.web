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

            PageIndex = jc.Params["page"].ToInt(1) - 1;

            string orderby = jc.Params["sort"];

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
                    val = jc.Params[key];

                    val = string.IsNullOrEmpty(val) ? string.Empty : val.Replace("'", "''").Replace("%", "");

                    base[key] = val.Trim();
                }

                return val;
            }
        }
    }
}
