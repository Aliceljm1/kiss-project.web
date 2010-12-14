using System;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using Kiss.Query;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// 分页控件
    /// </summary>
    [PersistChildren(false), ParseChildren(true)]
    public class Paging : Control
    {
        #region fields / props

        /// <summary>
        /// 页面扩展名（用于生成链接）
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// 查询条件
        /// </summary>
        [Browsable(false)]
        public QueryCondition QueryCondition { get; set; }

        private string _url;
        /// <summary>
        /// 链接根url
        /// </summary>
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        private string _prevText = "上一页";
        /// <summary>
        /// 上一页文字
        /// </summary>
        public string PrevText
        {
            get { return _prevText; }
            set { _prevText = value; }
        }

        private string _nextText = "下一页";
        /// <summary>
        /// 下一页文字
        /// </summary>
        public string NextText
        {
            get { return _nextText; }
            set { _nextText = value; }
        }

        /// <summary>
        /// 总是显示上一页链接
        /// </summary>
        public bool AlwaysShowPrev { get; set; }

        /// <summary>
        /// 总是显示下一页链接
        /// </summary>
        public bool AlwaysShowNext { get; set; }

        private int _numDispay = 8;
        /// <summary>
        /// 显示页码总数
        /// </summary>
        public int NumDispay
        {
            get { return _numDispay; }
            set { _numDispay = value; }
        }

        private int _numEdge = 1;
        /// <summary>
        /// 开始/结束页码数目（其它的页面用...替代）
        /// </summary>
        public int NumEdge
        {
            get { return _numEdge; }
            set { _numEdge = value; }
        }

        private int numPages
        {
            get
            {
                return Convert.ToInt32(Math.Ceiling((decimal)QueryCondition.TotalCount / QueryCondition.PageSize));
            }
        }

        private int[] getInterval
        {
            get
            {
                int half = Convert.ToInt32(Math.Ceiling((decimal)NumDispay / 2));
                int np = numPages;
                int upper_limit = np - NumDispay;
                int start = QueryCondition.PageIndex > half ? Math.Max(Math.Min(QueryCondition.PageIndex - half, upper_limit), 0) : 0;
                int end = QueryCondition.PageIndex > half ? Math.Min(QueryCondition.PageIndex + half, np) : Math.Min(NumDispay, np);

                return new int[] { start, end };
            }
        }

        /// <summary>
        /// ViewData键值，用于mvc风格的绑定
        /// </summary>
        public string DataKey { get; set; }

        /// <summary>
        /// 使用自定义的样式
        /// </summary>
        public bool UseCustomStyle { get; set; }

        /// <summary>
        /// 概要模板
        /// </summary>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public ITemplate SummaryTemplate { get; set; }

        /// <summary>
        /// 支持键盘分页
        /// </summary>
        public bool SupportKeyboard { get; set; }

        #endregion

        bool _isAjaxRequest = false;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _isAjaxRequest = JContext.Current.IsAjaxRequest;

            if (!UseCustomStyle && !_isAjaxRequest)
                ClientScriptProxy.Current.RegisterCssBlock(".pagination { float: right; margin: 10px 10px 10px 0; } .pagination a { border: 1px solid #AAAAEE; color: #1155BB; text-decoration: none; } .pagination a:hover { background-color: transparent; text-decoration: underline; } .pagination a, .pagination span { display: block; float: left; margin-bottom: 5px; margin-right: 5px; padding: 0.3em 0.5em; } .pagination .current { background: #2266BB none repeat scroll 0 0; border: 1px solid #AAAAEE; color: #FFFFFF; } .pagination span.prev, .pagination span.next { background: #FFFFFF none repeat scroll 0 0; border-color: #999999; color: #999999; } ", "pagingcss");
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            ClientScriptProxy.Current.RegisterJsResource(writer, "Kiss.Web.jQuery", "Kiss.Web.jQuery.pagenavi.js");

            if (QueryCondition == null)
                QueryCondition = JContext.Current.GetViewData(DataKey ?? "q") as QueryCondition;

            if (QueryCondition != null)
            {
                if (QueryCondition.PageSize == 0)
                    return;

                DrawLinks(writer);
            }

            ClientScriptProxy.Current.RegisterJsBlock(writer, "paging.keyboard", "$(function(){$('.pagination').pageNavi();});", true, true);
        }

        private void DrawLinks(HtmlTextWriter writer)
        {
            StringBuilder txt = new StringBuilder();
            int[] interval = getInterval;
            var np = numPages;
            if (np < 2)
                return;

            if (StringUtil.HasText(PrevText) && (QueryCondition.PageIndex > 0 || AlwaysShowPrev))
                txt.Append(appendItem(QueryCondition.PageIndex - 1, new string[] { PrevText, "prev" }));

            if (interval[0] > 0 && NumEdge > 0)
            {
                var end = Math.Min(NumEdge, interval[0]);
                for (int i = 0; i < end; i++)
                {
                    txt.Append(appendItem(i, null));
                }
                if (NumEdge < interval[0])
                    txt.Append("<span>...</span>");
            }

            for (int i = interval[0]; i < interval[1]; i++)
            {
                txt.Append(appendItem(i, null));
            }

            if (interval[1] < np && NumEdge > 0)
            {
                if (np - NumEdge > interval[1])
                {
                    txt.Append("<span>...</span>");
                }

                var begin = Math.Max(np - NumEdge, interval[1]);
                for (int i = begin; i < np; i++)
                {
                    txt.Append(appendItem(i, null));
                }
            }

            if (StringUtil.HasText(NextText) && (QueryCondition.PageIndex < np - 1 || AlwaysShowNext))
                txt.Append(appendItem(QueryCondition.PageIndex + 1, new string[] { NextText, "next" }));

            if (!UseCustomStyle)
                writer.Write("<div class='pagination'>");

            if (SummaryTemplate != null)
            {
                Control t = new Control();
                SummaryTemplate.InstantiateIn(t);
                t.RenderControl(writer);
            }

            writer.Write(txt.ToString());

            if (!UseCustomStyle)
                writer.Write("</div>");
        }

        public string appendItem(int page_id, string[] appendopts)
        {
            page_id = page_id < 0 ? 0 : (page_id < numPages ? page_id : numPages - 1);
            if (appendopts == null)
                appendopts = new string[] { (page_id + 1).ToString(), "" };
            if (page_id == QueryCondition.PageIndex)
                return string.Format("<span class='current {1}'>{0}</span>", appendopts[0], appendopts[1]);
            return string.Format("<a href='{2}' {1}>{0}</a>", appendopts[0],
                StringUtil.HasText(appendopts[1]) ? string.Format("class='{0}'", appendopts[1]) : string.Empty,
               FormatUrl(page_id));

        }

        private string _virtualPath;
        protected string VirtualPath
        {
            get
            {
                if (_virtualPath == null)
                    _virtualPath = JContext.Current.Site.VirtualPath;
                return _virtualPath;
            }
        }

        private string FormatUrl(int page_id)
        {
            if (_isAjaxRequest && StringUtil.IsNullOrEmpty(Url))
                return "#";

            if (StringUtil.HasText(Url))
            {
                if (Url.Contains("?"))
                    return string.Format(Url, page_id + 1);
                return string.Format(Url + Context.Request.Url.Query, page_id + 1);
            }
            else
            {
                if (StringUtil.HasText(Extension) && !Context.Request.Url.AbsolutePath.EndsWith(Extension))
                    return string.Format("{0}/{1}{2}", Context.Request.Url.AbsolutePath, page_id + 1, Extension);

                //string url = StringUtil.CombinUrl( VirtualPath, HttpContext.Current.Request.Url.PathAndQuery );

                string url = HttpContext.Current.Request.Url.PathAndQuery;

                int i = url.LastIndexOf("/");

                int j = url.LastIndexOf("?");
                return string.Format("{0}{1}{2}{3}",
                    url.Substring(0, i + 1),
                    page_id + 1,
                    Extension,
                    (j == -1) ? string.Empty : url.Substring(j)
                  );
            }
        }
    }
}
