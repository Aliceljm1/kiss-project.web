using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace Kiss.Web.Optimization
{
    /// <summary>
    /// hooks onto the Response.Filter property of the current request 
    /// and tries to compress the output, based on what the browser supports
    /// </summary>
    /// <remarks>
    /// <p>We already support gzip and deflate (aka zlib), if you'd like to add 
    /// support for compress (which uses LZW, which is licensed), add in another
    /// class that inherits from HttpFilter to do the work.</p>
    /// 
    /// <p>This module checks the Accept-Encoding HTTP header to determine if the
    /// client actually supports any notion of compression.  Currently, we support
    /// the deflate (zlib) and gzip compression schemes.  I chose to not implement
    /// compress because it uses lzw which requires a license from 
    /// Unisys.  For more information about the common compression types supported,
    /// see http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.11 for details.</p> 
    /// </remarks>
    /// <seealso cref="CompressingFilter"/>
    /// <seealso cref="Stream"/>
    sealed class OptimizationModule
    {
        const string INSTALLED_KEY = "optimizationmodule.attemptedinstall";
        static readonly object INSTALLED_TAG = new object();

        static readonly List<string> VALID_CONTENTTYPES = new List<string>() { "text/html", "text/css", "text/javascript" };

        public OptimizationModule()
        {
        }

        ~OptimizationModule()
        {
            Stop();
        }

        internal static void SetOptimized(HttpContext context)
        {
            context.Items[INSTALLED_KEY] = INSTALLED_TAG;
        }

        /// <summary>
        /// EventHandler that gets ahold of the current request context and attempts to compress the output.
        /// </summary>
        /// <param name="sender">The <see cref="HttpApplication"/> that is firing this event.</param>
        /// <param name="e">Arguments to the event</param>
        void OptimizeContent(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            string contentType = app.Context.Response.ContentType;
            if (string.IsNullOrEmpty(contentType)) return;

            contentType = contentType.ToLowerInvariant();

            if (app.Context.AllErrors != null || !VALID_CONTENTTYPES.Contains(contentType) || app.Context.Response.IsRequestBeingRedirected)
                return;

            // only do this if we havn't already attempted an install.  This prevents PreSendRequestHeaders from
            // trying to add this item way to late.  We only want the first run through to do anything.
            // also, we use the context to store whether or not we've attempted an add, as it's thread-safe and
            // scoped to the request.  An instance of this module can service multiple requests at the same time,
            // so we cannot use a member variable.
            if (app.Context.Items.Contains(INSTALLED_KEY))
                return;

            // log the install attempt in the HttpContext
            // must do this first as several IF statements
            // below skip full processing of this method
            SetOptimized(app.Context);

            addCompressFilter(app);

            switch (contentType)
            {
                case "text/html":
                    app.Response.Filter = new HtmlMinifierStream(app.Response.Filter);
                    app.Response.Filter = new InlineStyleMinifierStream(app.Response.Filter);
                    app.Response.Filter = new InlineJavascriptMinifierStream(app.Response.Filter);
                    break;
                case "text/css":
                    app.Response.Filter = new StyleMinifierStream(app.Response.Filter);
                    break;
                case "text/javascript":
                    app.Response.Filter = new JavascriptMinifierStream(app.Response.Filter);
                    break;
                default:
                    break;
            }
        }

        void addCompressFilter(HttpApplication app)
        {
            // fix to handle caching appropriately
            // see http://www.pocketsoap.com/weblog/2003/07/1330.html
            // Note, this header is added only when the request
            // has the possibility of being compressed...
            // i.e. it is not added when the request is excluded from
            // compression by CompressionLevel, Path, or MimeType
            app.Response.Cache.VaryByHeaders["Accept-Encoding"] = true;

            // grab an array of algorithm;q=x, algorith;q=x style values
            string acceptedTypes = app.Request.Headers["Accept-Encoding"];

            // if we couldn't find the header, bail out
            if (acceptedTypes == null)
                return;

            // the actual types could be , delimited.  split 'em out.
            string[] types = acceptedTypes.Split(',');

            CompressingFilter filter = GetFilterForScheme(types, app.Response.Filter);

            // if we didn't find a filter, bail out
            if (filter == null)
                return;

            // if we get here, we found a viable filter.
            // set the filter and change the Content-Encoding header to match so the client can decode the response
            app.Response.Filter = filter;
        }

        /// <summary>
        /// Get ahold of a <see cref="CompressingFilter"/> for the given encoding scheme.
        /// If no encoding scheme can be found, it returns null.
        /// </summary>
        /// <remarks>
        /// See http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.3 for details
        /// on how clients are supposed to construct the Accept-Encoding header.  This
        /// implementation follows those rules, though we allow the server to override
        /// the preference given to different supported algorithms.  I'm doing this as 
        /// I would rather give the server control over the algorithm decision than 
        /// the client.  If the clients send up * as an accepted encoding with highest
        /// quality, we use the preferred algorithm as specified in the config file.
        /// </remarks>
        public static CompressingFilter GetFilterForScheme(string[] schemes, Stream output)
        {
            bool foundDeflate = false;
            bool foundGZip = false;
            bool foundStar = false;

            float deflateQuality = 0f;
            float gZipQuality = 0f;
            float starQuality = 0f;

            bool isAcceptableDeflate;
            bool isAcceptableGZip;
            bool isAcceptableStar;

            for (int i = 0; i < schemes.Length; i++)
            {
                string acceptEncodingValue = schemes[i].Trim().ToLower();

                if (acceptEncodingValue.StartsWith("deflate"))
                {
                    foundDeflate = true;

                    float newDeflateQuality = GetQuality(acceptEncodingValue);
                    if (deflateQuality < newDeflateQuality)
                        deflateQuality = newDeflateQuality;
                }
                else if (acceptEncodingValue.StartsWith("gzip") || acceptEncodingValue.StartsWith("x-gzip"))
                {
                    foundGZip = true;

                    float newGZipQuality = GetQuality(acceptEncodingValue);
                    if (gZipQuality < newGZipQuality)
                        gZipQuality = newGZipQuality;
                }

                else if (acceptEncodingValue.StartsWith("*"))
                {
                    foundStar = true;

                    float newStarQuality = GetQuality(acceptEncodingValue);
                    if (starQuality < newStarQuality)
                        starQuality = newStarQuality;
                }
            }

            isAcceptableStar = foundStar && (starQuality > 0);
            isAcceptableDeflate = (foundDeflate && (deflateQuality > 0)) || (!foundDeflate && isAcceptableStar);
            isAcceptableGZip = (foundGZip && (gZipQuality > 0)) || (!foundGZip && isAcceptableStar);

            if (isAcceptableDeflate && !foundDeflate)
                deflateQuality = starQuality;

            if (isAcceptableGZip && !foundGZip)
                gZipQuality = starQuality;


            // do they support any of our compression methods?
            if (!(isAcceptableDeflate || isAcceptableGZip || isAcceptableStar))
            {
                return null;
            }

            // if deflate is better according to client
            if (isAcceptableDeflate && (!isAcceptableGZip || (deflateQuality > gZipQuality)))
                return new DeflateFilter(output, CompressionLevels.High);

            // if gzip is better according to client
            if (isAcceptableGZip && (!isAcceptableDeflate || (deflateQuality < gZipQuality)))
                return new GZipFilter(output);

            // if we're here, the client either didn't have a preference or they don't support compression
            if (isAcceptableDeflate || isAcceptableStar)
                return new DeflateFilter(output, CompressionLevels.High);
            if (isAcceptableGZip)
                return new GZipFilter(output);

            // return null. we couldn't find a filter.
            return null;
        }

        static float GetQuality(string acceptEncodingValue)
        {
            int qParam = acceptEncodingValue.IndexOf("q=");

            if (qParam >= 0)
            {
                float val = 0.0f;
                try
                {
                    val = float.Parse(acceptEncodingValue.Substring(qParam + 2, acceptEncodingValue.Length - (qParam + 2)));
                }
                catch (FormatException)
                {

                }
                return val;
            }
            else
                return 1;
        }

        public void Stop()
        {
            EventBroker broker = EventBroker.Instance;

            broker.ReleaseRequestState -= OptimizeContent;
            broker.PreSendRequestHeaders -= OptimizeContent;
        }

        public void Start()
        {
            Stop();

            EventBroker broker = EventBroker.Instance;

            broker.ReleaseRequestState += OptimizeContent;
            broker.PreSendRequestHeaders += OptimizeContent;
        }
    }
}
