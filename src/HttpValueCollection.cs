#region File Comment
//+-------------------------------------------------------------------+
//+ File Created:   2009-09-23
//+-------------------------------------------------------------------+
//+ History:
//+-------------------------------------------------------------------+
//+ 2009-09-23		zhli Comment Created
//+-------------------------------------------------------------------+
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace Kiss.Web
{
    [Serializable]
    public class HttpValueCollection : NameValueCollection
    {
        // Methods
        public HttpValueCollection ( )
            : base ( StringComparer.OrdinalIgnoreCase )
        {
        }

        public HttpValueCollection ( int capacity )
            : base ( capacity , ( IEqualityComparer ) StringComparer.OrdinalIgnoreCase )
        {
        }

        public HttpValueCollection ( string str )
            : this ( str , false , false , Encoding.UTF8 )
        {
        }

        public HttpValueCollection ( SerializationInfo info , StreamingContext context )
            : base ( info , context )
        {
        }

        public HttpValueCollection ( string str , bool urlencoded , Encoding encoding )
            : this ( str , false , urlencoded , encoding )
        {
        }

        public HttpValueCollection ( string str , bool readOnly , bool urlencoded , Encoding encoding )
            : base ( StringComparer.OrdinalIgnoreCase )
        {
            if ( !string.IsNullOrEmpty ( str ) )
            {
                this.FillFromString ( str , urlencoded , encoding );
            }
            base.IsReadOnly = readOnly;
        }

        internal void Add ( HttpCookieCollection c )
        {
            int count = c.Count;
            for ( int i = 0 ; i < count ; i++ )
            {
                HttpCookie cookie = c.Get ( i );
                base.Add ( cookie.Name , cookie.Value );
            }
        }

        internal void FillFromEncodedBytes ( byte[ ] bytes , Encoding encoding )
        {
            int num = ( bytes != null ) ? bytes.Length : 0;
            for ( int i = 0 ; i < num ; i++ )
            {
                string str;
                string str2;
                int offset = i;
                int num4 = -1;
                while ( i < num )
                {
                    byte num5 = bytes[ i ];
                    if ( ( num5 == 0x3d ) && ( num4 < 0 ) )
                    {
                        num4 = i;
                    }
                    i++;
                }
                if ( num4 >= 0 )
                {
                    str = HttpUtility.UrlDecode ( bytes , offset , num4 - offset , encoding );
                    str2 = HttpUtility.UrlDecode ( bytes , num4 + 1 , ( i - num4 ) - 1 , encoding );
                }
                else
                {
                    str = null;
                    str2 = HttpUtility.UrlDecode ( bytes , offset , i - offset , encoding );
                }
                base.Add ( str , str2 );
                if ( ( i == ( num - 1 ) ) && ( bytes[ i ] == 0x26 ) )
                {
                    base.Add ( null , string.Empty );
                }
            }
        }

        public void FillFromString ( string s )
        {
            this.FillFromString ( s , false , null );
        }

        public void FillFromString ( string s , bool urlEncoded , Encoding encoding )
        {
            if ( !string.IsNullOrEmpty ( s ) )
            {
                for ( int i = 0 ; i < s.Length ; i++ )
                {
                    int startIndex = i;
                    int num3 = -1;
                    while ( i < s.Length )
                    {
                        if ( s[ i ] == '=' )
                        {
                            if ( num3 < 0 )
                            {
                                num3 = i;
                            }
                        }
                        else if ( s[ i ] == '&' )
                        {
                            break;
                        }
                        i++;
                    }
                    string str = null;
                    string str2 = null;
                    if ( num3 >= 0 )
                    {
                        str = s.Substring ( startIndex , num3 - startIndex );
                        str2 = s.Substring ( num3 + 1 , ( i - num3 ) - 1 );
                    }
                    else
                    {
                        str2 = s.Substring ( startIndex , i - startIndex );
                    }
                    if ( urlEncoded )
                    {
                        base.Add ( HttpUtility.UrlDecode ( str , encoding ) , HttpUtility.UrlDecode ( str2 , encoding ) );
                    }
                    else
                    {
                        base.Add ( str , str2 );
                    }
                    if ( ( i == ( s.Length - 1 ) ) && ( s[ i ] == '&' ) )
                    {
                        base.Add ( null , string.Empty );
                    }
                }
            }
        }

        internal void MakeReadOnly ( )
        {
            base.IsReadOnly = true;
        }

        internal void MakeReadWrite ( )
        {
            base.IsReadOnly = false;
        }

        internal void Reset ( )
        {
            base.Clear ( );
        }

        public override string ToString ( )
        {
            return this.ToString ( true );
        }

        public virtual string ToString ( bool urlencoded )
        {
            return this.ToString ( urlencoded , null );
        }

        public virtual string ToString ( bool urlEncoded , IDictionary excludeKeys )
        {
            if ( this.Count == 0 )
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder ( );
            bool flag = ( excludeKeys != null ) && ( excludeKeys[ "__VIEWSTATE" ] != null );
            for ( int i = 0 ; i < this.Count ; i++ )
            {
                string key = this.GetKey ( i );
                if ( ( ( !flag || ( key == null ) ) || !key.StartsWith ( "__VIEWSTATE" , StringComparison.Ordinal ) ) && ( ( ( excludeKeys == null ) || ( key == null ) ) || ( excludeKeys[ key ] == null ) ) )
                {
                    if ( urlEncoded )
                    {
                        key = HttpUtility.UrlEncodeUnicode ( key );
                    }
                    key = !string.IsNullOrEmpty ( key ) ? ( key + "=" ) : string.Empty;
                    ArrayList list = ( ArrayList ) base.BaseGet ( i );
                    if ( builder.Length > 0 )
                    {
                        builder.Append ( '&' );
                    }
                    int num2 = ( list != null ) ? list.Count : 0;
                    if ( num2 == 1 )
                    {
                        builder.Append ( key );
                        string str = ( string ) list[ 0 ];
                        if ( urlEncoded )
                        {
                            str = HttpUtility.UrlEncodeUnicode ( str );
                        }
                        builder.Append ( str );
                    }
                    else if ( num2 == 0 )
                    {
                        builder.Append ( key );
                    }
                    else
                    {
                        for ( int j = 0 ; j < num2 ; j++ )
                        {
                            if ( j > 0 )
                            {
                                builder.Append ( '&' );
                            }
                            builder.Append ( key );
                            string str3 = ( string ) list[ j ];
                            if ( urlEncoded )
                            {
                                str3 = HttpUtility.UrlEncodeUnicode ( str3 );
                            }
                            builder.Append ( str3 );
                        }
                    }
                }
            }
            return builder.ToString ( );
        }
    }
}
