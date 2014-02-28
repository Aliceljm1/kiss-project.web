using System;

namespace Kiss.Web.UrlMapping
{
    [Serializable]
    public class UrlMappingException : KissException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public UrlMappingException ( ) { }
        public UrlMappingException ( string message ) : base ( message ) { }
        public UrlMappingException ( string message, Exception inner ) : base ( message, inner ) { }
        protected UrlMappingException (
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context )
            : base ( info, context ) { }
    }
}
