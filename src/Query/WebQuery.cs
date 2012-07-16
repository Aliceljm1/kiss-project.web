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
        private NameValueCollection param;

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

            param = JContext.Current.Params;

            PageIndex = param["page"].ToInt(1) - 1;

            string orderby = param["sort"];

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
                if (val == null && param != null)// get from context
                {
                    val = param[key];

                    val = string.IsNullOrEmpty(val) ? string.Empty : val.Replace("'", "''").Replace("%", "");

                    base[key] = val.Trim().Trim('+').Replace('+',' ');
                }

                return val;
            }
        }
    }
}
