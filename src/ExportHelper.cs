using System;
using System.IO;
using System.Text;
using Kiss.Query;
using Kiss.Utils;
using Kiss.Web.Controls;
using Kiss.Web.Mvc;

namespace Kiss.Web
{
    public class ExportHelper : IDisposable
    {
        bool isexport;
        JContext jc;
        string filename;
        string content = "$!contentModel.renderTable()";

        public string SkinNamePostFix { get; set; }

        public string Content { get { return content; } set { content = value; } }

        public QueryCondition QC { get; set; }

        public bool IsExport { get { return isexport; } }

        public ExportHelper(string filename, JContext jc)
            : this(10, filename, jc)
        {
        }

        public ExportHelper(int? pageSize, string filename, JContext jc)
            : this(StringUtil.HasText(jc.QueryString["export"]), pageSize, filename, jc)
        {
        }

        public ExportHelper(bool isexport, int? pageSize, string filename, JContext jc)
        {
            this.isexport = isexport;
            this.filename = filename;
            this.jc = jc;

            QC = new WebQuery();
            QC.LoadCondidtion();

            if (this.isexport)
            {
                if (jc.QueryString["export"] == "all")
                {
                    QC.NoPaging();
                }
                else if (pageSize != null)
                {
                    QC.PageSize = pageSize.Value;
                }
            }
        }

        public void Dispose()
        {
            if (!this.isexport) return;

            jc.RenderContent = false;

            StringBuilder sb = new StringBuilder();

            sb.Append("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
            sb.Append("<head>");
            sb.Append("<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\">");
            sb.Append("<meta name=ProgId content=Excel.Sheet>");
            sb.Append("<!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>" + this.filename + "</x:Name><x:WorksheetOptions><x:Print><x:ValidPrinterInfo /></x:Print></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]-->");
            sb.Append("</head>");

            sb.Append("<body>");

            if (string.IsNullOrEmpty(SkinNamePostFix))
            {
                try
                {
                    ITemplateEngine te = ServiceLocator.Instance.Resolve<ITemplateEngine>();

                    using (StringWriter sw = new StringWriter())
                    {
                        te.Process(JContext.Current.ViewData, string.Empty, sw, "$!contentModel.renderTable()");

                        sb.Append(sw.GetStringBuilder().ToString());
                    }
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger<ExportHelper>().Error(ex.Message);
                }
            }
            else
            {
                sb.Append(new TemplatedControl(jc.Navigation.Action + SkinNamePostFix).Execute());
            }

            sb.Append("</body></html>");

            new FileContentResult(Encoding.UTF8.GetBytes(sb.ToString()), "application/vnd.ms-excel") { FileDownloadName = filename }.ExecuteResult(jc);
        }
    }
}
