#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-09-02
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-09-02		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System.Web;

namespace Kiss.Web
{
    /// <summary>
    /// Provides access to shared objects stored in the http context.
    /// </summary>
    /// <typeparam name="T">The type of object to get. This class only holds one instance of such an object for each request.</typeparam>
    public class HttpContextItem<T> : HttpContextItem where T : class
    {
        /// <summary>
        /// The instance of the object of the type as provided by <see cref="HttpContextItem.Accessor"/>.
        /// </summary>
        public static T Instance
        {
            get { return Accessor.Get ( typeof ( T ) ) as T; }
            set { Accessor.Set ( typeof ( T ), value ); }
        }
    }

    /// <summary>
    /// Provides global access to the http context item accessor. 
    /// </summary>
    public class HttpContextItem
    {
        static HttpContextItem ( )
        {
            Accessor = new HttpContextItemAccessor ( );
        }

        /// <summary>
        /// Wraps access to the http context's item store.
        /// </summary>
        public static HttpContextItemAccessor Accessor
        {
            get { return Singleton<HttpContextItemAccessor>.Instance; }
            protected set { Singleton<HttpContextItemAccessor>.Instance = value; }
        }
    }

    /// <summary>
    /// get/set data from http context item
    /// </summary>
    public class HttpContextItemAccessor
    {
        /// <summary>
        /// get data from http context item
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get ( object key )
        {
            return HttpContext.Current.Items[ key ];
        }

        /// <summary>
        /// set data to http context item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="instance"></param>
        public void Set ( object key, object instance )
        {
            HttpContext.Current.Items[ key ] = instance;
        }
    }
}
