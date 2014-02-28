using System.Web;

namespace Kiss.Web.Utils
{
    public delegate T GetObj<T> ( );

    /// <summary>
    /// util class 
    /// </summary>
    public static class HttpContextUtil
    {
        public static T GetAndSave<T> ( string key , GetObj<T> getter )
        {
            object o = HttpContext.Current.Items[ key ];
            if ( o == null )
            {
                o = getter ( );
                HttpContext.Current.Items[ key ] = o;
            }

            if ( o == null )
                return default ( T );

            return ( T ) o;
        }
    }
}
