using System;
using System.Web;

namespace Kiss.Web.Mvc
{
    public class FileContentResult : FileResult
    {
        public FileContentResult(byte[] fileContents, string contentType)
            : base(contentType)
        {
            if (fileContents == null)
            {
                throw new ArgumentNullException("fileContents");
            }

            FileContents = fileContents;
        }

        public byte[] FileContents
        {
            get;
            private set;
        }

        protected override void WriteFile(HttpResponse response)
        {
            response.OutputStream.Write(FileContents, 0, FileContents.Length);
        }
    }
}
