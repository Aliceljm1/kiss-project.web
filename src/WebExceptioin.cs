using System;

namespace Kiss.Web
{
    [Serializable]
    public class WebException : KissException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public WebException() { }
        public WebException(string message) : base(message) { }
        public WebException(string message, Exception inner) : base(message, inner) { }
        public WebException(string format, params object[] objs) : base(format, objs) { }
        protected WebException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
