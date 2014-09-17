using System;
using System.Web;

namespace Kiss.Web.Mvc
{
    public class FilePathResult : FileResult
    {
        public FilePathResult(string fileName, string contentType)
            : base(contentType)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("fileName");
            }

            FileName = fileName;
        }

        public string FileName
        {
            get;
            private set;
        }

        protected override void WriteFile(HttpResponse response)
        {
            response.TransmitFile(FileName);
        }

    }
}
