using System;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace Kiss.Web.Mvc
{
    public abstract class FileResult : ActionResult
    {
        protected FileResult(string contentType)
        {
            if (String.IsNullOrEmpty(contentType))
            {
                throw new ArgumentException("contentType");
            }

            ContentType = contentType;
        }

        private string _fileDownloadName;

        public string ContentType
        {
            get;
            private set;
        }

        public string FileDownloadName
        {
            get
            {
                return _fileDownloadName ?? String.Empty;
            }
            set
            {
                _fileDownloadName = value;
            }
        }

        public override void ExecuteResult(JContext jc)
        {
            if (jc == null)
            {
                throw new ArgumentNullException("jc");
            }

            HttpResponse response = jc.Context.Response;
            response.ContentType = ContentType;
            response.ContentEncoding = Encoding.UTF8;

            if (!String.IsNullOrEmpty(FileDownloadName))
            {
                // From RFC 2183, Sec. 2.3:
                // The sender may want to suggest a filename to be used if the entity is
                // detached and stored in a separate file. If the receiving MUA writes
                // the entity to a file, the suggested filename should be used as a
                // basis for the actual filename, where possible.
                string headerValue = GetHeaderValue(FileDownloadName);
                response.AddHeader("Content-Disposition", headerValue);
            }

            WriteFile(response);

            response.End();
        }

        protected abstract void WriteFile(HttpResponse response);

        public static string GetHeaderValue(string fileName)
        {
            if (HttpContext.Current.Request.UserAgent.IndexOf("msie", StringComparison.InvariantCultureIgnoreCase) != -1)
                fileName = HttpUtility.UrlPathEncode(fileName);

            return string.Format("attachment; filename=\"{0}\"", fileName);
        }
    }
}
