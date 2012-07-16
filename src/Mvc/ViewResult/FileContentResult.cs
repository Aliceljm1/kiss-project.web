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
            // 必须设置content-length，否则nginx+fastcgi-mono-server2环境下，下载文件时会出现奇怪的bug
            response.AddHeader("Content-Length", FileContents.Length.ToString());
            
            response.BinaryWrite(FileContents);
        }
    }
}
