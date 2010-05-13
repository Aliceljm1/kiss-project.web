
using System.Web;

namespace Kiss.Web.Mvc
{
    public class SendFileResult : ActionResult
    {
        public string ContentType { get; set; }
        public byte[] FileData { get; set; }
        public string RemoteFileName { get; set; }
        public string LocalFileName { get; set; }

        public SendFileResult(string localFileName, string contentType)
        {
            ContentType = contentType;
            LocalFileName = localFileName;
        }

        public SendFileResult(byte[] fileData, string contentType)
        {
            ContentType = contentType;
            FileData = fileData;
        }

        public SendFileResult(string localFileName, string contentType, string remoteFileName)
        {
            ContentType = contentType;
            LocalFileName = localFileName;
            RemoteFileName = remoteFileName;
        }

        public SendFileResult(byte[] fileData, string contentType, string remotefileName)
        {
            RemoteFileName = remotefileName;
            ContentType = contentType;
            FileData = fileData;
        }

        public override void ExecuteResult(JContext jc)
        {
            HttpContext httpContext = jc.Context;

            httpContext.Response.ContentType = ContentType;

            if (RemoteFileName != null)
                httpContext.Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + RemoteFileName + "\"");

            if (FileData != null)
                httpContext.Response.BinaryWrite(FileData);
            else
                httpContext.Response.WriteFile(LocalFileName);
        }
    }
}
