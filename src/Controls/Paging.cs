using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        private int _numDisplay = 8;
        /// <summary>
        /// 显示页码总数
        /// </summary>
        public int NumDisplay
        {
            get { return _numDisplay; }
            set { _numDisplay = value; }
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

        /// <summary>
        /// ViewData键值，用于mvc风格的绑定
        /// </summary>
        public string DataKey { get; set; }

        /// <summary>
        /// 模板
        /// </summary>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public ITemplate Template { get; set; }

        #endregion

        bool _isAjaxRequest = false;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _isAjaxRequest = JContext.Current.IsAjaxRequest;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            if (QueryCondition == null)
                QueryCondition = JContext.Current.GetViewData(DataKey ?? "q") as QueryCondition;

            if (QueryCondition != null)
            {
                if (!QueryCondition.Paging)
                    return;

                DrawLinks(writer);
            }
        }

        public ArrayList GetDataSource()
        {
            if (QueryCondition.Paging)
                return GetDataSource(QueryCondition, NumDisplay, NumEdge, AlwaysShowPrev, AlwaysShowNext);

            return null;
        }

        private void DrawLinks(HtmlTextWriter writer)
        {
            if (Template != null)
            {
                ArrayList datasource = GetDataSource();

                if (datasource == null || datasource.Count == 0) return;

                Control ctrl = new Control() { };

                Template.InstantiateIn(ctrl);

                string content = string.Empty;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (HtmlTextWriter htmlWriter = new HtmlTextWriter(new StreamWriter(ms)))
                    {
                        ctrl.RenderControl(htmlWriter);
                        htmlWriter.Flush();
                    }

                    using (StreamReader rdr = new StreamReader(ms, Encoding.UTF8))
                    {
                        rdr.BaseStream.Position = 0;
                        content = rdr.ReadToEnd();
                    }
                }

                try
                {
                    ITemplateEngine te = ServiceLocator.Instance.Resolve<ITemplateEngine>();

                    using (StringWriter sw = new StringWriter())
                    {
                        Dictionary<string, object> di = new Dictionary<string, object>(JContext.Current.ViewData);
                        di["pagings"] = datasource;

                        te.Process(di, string.Empty, sw, content);

                        writer.Write(sw.GetStringBuilder().ToString());
                    }
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger<Paging>().Error(ex.Message);
                    ExceptionUtil.WriteException(ex);
                }
            }
            else
            {
                StringBuilder txt = new StringBuilder();
                int[] interval = getInterval(QueryCondition, NumDisplay);
                var np = get_numPages(QueryCondition);
                if (np < 2)
                    return;

                if (StringUtil.HasText(PrevText) && (QueryCondition.PageIndex > 0 || AlwaysShowPrev))
                    txt.Append(appendItem(QueryCondition.PageIndex - 1, np, new string[] { PrevText, "prev" }));

                if (interval[0] > 0 && NumEdge > 0)
                {
                    var end = Math.Min(NumEdge, interval[0]);
                    for (int i = 0; i < end; i++)
                    {
                        txt.Append(appendItem(i, np, null));
                    }
                    if (NumEdge < interval[0])
                        txt.Append("<span>...</span>");
                }

                for (int i = interval[0]; i < interval[1]; i++)
                {
                    txt.Append(appendItem(i, np, null));
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
                        txt.Append(appendItem(i, np, null));
                    }
                }

                if (StringUtil.HasText(NextText) && (QueryCondition.PageIndex < np - 1 || AlwaysShowNext))
                    txt.Append(appendItem(QueryCondition.PageIndex + 1, np, new string[] { NextText, "next" }));

                writer.Write("<div class='pagination'>");

                writer.Write(txt.ToString());

                writer.Write("</div>");

                if (_isAjaxRequest)
                    ClientScriptProxy.Current.RegisterJsBlock(writer, "_paging_ajax_", "jQuery.paging.ajax()", true, true);
            }
        }

        public static ArrayList GetDataSource(QueryCondition qc, int numDisplay, int numEdge, bool AlwaysShowPrev, bool AlwaysShowNext)
        {
            JContext jc = JContext.Current;

            ArrayList datasource = new ArrayList();

            StringBuilder txt = new StringBuilder();
            int[] interval = getInterval(qc, numDisplay);
            var np = get_numPages(qc);
            if (np < 2)
                return datasource;

            if (qc.PageIndex > 0 || AlwaysShowPrev)
                datasource.Add(new { pageindex = qc.PageIndex - 1, type = "prev", href = FormatUrl(qc.PageIndex - 1, null, jc.IsAjaxRequest) });

            if (interval[0] > 0 && numEdge > 0)
            {
                var end = Math.Min(numEdge, interval[0]);
                for (int i = 0; i < end; i++)
                {
                    datasource.Add(new { pageindex = i, type = "item", href = FormatUrl(i, null, jc.IsAjaxRequest) });
                }
                if (numEdge < interval[0])
                    datasource.Add(new { type = "ellipsis" });
            }

            for (int i = interval[0]; i < interval[1]; i++)
            {
                datasource.Add(new { pageindex = i, type = "item", href = FormatUrl(i, null, jc.IsAjaxRequest) });
            }

            if (interval[1] < np && numEdge > 0)
            {
                if (np - numEdge > interval[1])
                {
                    datasource.Add(new { type = "ellipsis" });
                }

                var begin = Math.Max(np - numEdge, interval[1]);
                for (int i = begin; i < np; i++)
                {
                    datasource.Add(new { pageindex = i, type = "item", href = FormatUrl(i, null, jc.IsAjaxRequest) });
                }
            }

            if (qc.PageIndex < np - 1 || AlwaysShowNext)
                datasource.Add(new { pageindex = qc.PageIndex + 1, type = "next", href = FormatUrl(qc.PageIndex + 1, null, jc.IsAjaxRequest) });

            return datasource;
        }

        public string appendItem(int page_id, int numPages, string[] appendopts)
        {
            page_id = page_id < 0 ? 0 : (page_id < numPages ? page_id : numPages - 1);
            if (appendopts == null)
                appendopts = new string[] { (page_id + 1).ToString(), "" };
            if (page_id == QueryCondition.PageIndex)
                return string.Format("<span class='current {1}'>{0}</span>", appendopts[0], appendopts[1]);
            return string.Format("<a href='{2}' {1}>{0}</a>", appendopts[0],
                StringUtil.HasText(appendopts[1]) ? string.Format("class='{0}'", appendopts[1]) : string.Empty,
               FormatUrl(page_id, Url, _isAjaxRequest));
        }

        private static string FormatUrl(int page_id, string Url, bool isAjaxRequest)
        {
            if (isAjaxRequest && StringUtil.IsNullOrEmpty(Url))
                return "#";

            HttpContext httpContext = HttpContext.Current;

            if (StringUtil.HasText(Url))
            {
                if (Url.Contains("?"))
                    return string.Format(Url, page_id + 1);
                return string.Format(Url + httpContext.Request.Url.Query, page_id + 1);
            }
            else
            {
                Web.Url url = new Url(httpContext.Request.Url.PathAndQuery);

                int i = url.Path.LastIndexOf("/");

                return string.Format("{0}{1}{2}{3}",
                    url.Path.Substring(0, i + 1),
                    page_id + 1,
                    url.Extension,
                    string.IsNullOrEmpty(url.Query) ? string.Empty : "?" + url.Query
                  );
            }
        }

        private static int get_numPages(QueryCondition qc)
        {
            return Convert.ToInt32(Math.Ceiling((decimal)qc.TotalCount / qc.PageSize));
        }

        private static int[] getInterval(QueryCondition qc, int numDisplay)
        {
            int half = Convert.ToInt32(Math.Ceiling((decimal)numDisplay / 2));
            int np = get_numPages(qc);
            int upper_limit = np - numDisplay;
            int start = qc.PageIndex > half ? Math.Max(Math.Min(qc.PageIndex - half, upper_limit), 0) : 0;
            int end = qc.PageIndex > half ? Math.Min(qc.PageIndex + half, np) : Math.Min(numDisplay, np);

            return new int[] { start, end };
        }
    }
}
