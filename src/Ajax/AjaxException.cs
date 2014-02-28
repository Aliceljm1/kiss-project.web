using System;

namespace Kiss.Web.Ajax
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class AjaxException : KissException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public AjaxException() { }
        public AjaxException( string message ) : base( message ) { }
        public AjaxException( string message, Exception inner ) : base( message, inner ) { }
        protected AjaxException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context )
            : base( info, context ) { }
    }
}
