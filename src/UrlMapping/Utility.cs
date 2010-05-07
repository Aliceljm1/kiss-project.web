
using System.Text.RegularExpressions;

namespace Kiss.Web.UrlMapping
{
    public class Utility
    {
        public static string GetHref(string redirection)
        {
            if (string.IsNullOrEmpty(redirection))
                redirection = "~/default.aspx";
            else if (redirection.IndexOf('.') == -1)// master file
                redirection = string.Format("~/default.aspx?MasterFile={0}", redirection);
            return redirection;
        }

        /// <summary>
        /// Creates a <see cref="UrlMappingItem" /> given a name, templated URL string, and redirection string.
        /// </summary>
        /// <param name="name">A name for this mapping item</param>
        /// <param name="templatedUrl">A template for URLs to be matched</param>
        /// <param name="redirection">The redirection that the UrlMappingModule should apply when incoming URLs match the template.</param>
        /// <param name="qsBehavior">defines how the UrlMappingModule should treat querystring values on an incoming URL for pattern matching; this is typically provided through declarative configuration</param>
        /// <returns>The created <see cref="UrlMappingItem" /></returns>
        /// <remarks>
        /// <para>
        /// The template URL string is relative to the web application and may or may not include
        /// the "~/" characters at the beginning.  The template may indicate an exact URL to match,
        /// for example <i>Reports/Summary.aspx</i> or may include token names surrounded
        /// by [SquareBrackets] for dynamic matching, such as <i>Reports/[ID]/[Action].aspx</i>.
        /// When dynamic templates are matched in runtime by the UrlMappingModule, dynamic tokens
        /// are appended to the redirection string as querystring items, 
        /// such as <i>?ID=<i>xxx</i>&amp;Action=<i>xxx</i></i>.
        /// </para>
        /// <para>
        /// The redirection string may be an absolute URL, such as <i>http://www.microsoft.com</i>,
        /// a server-relative URL beginning with a "/", such as <i>/AnotherAppOnThisServer/page.aspx</i>,
        /// or an application-relative URL to a concrete resource, optionally beginning with a "~/", such as
        /// <i>~/ReportSummary.aspx</i> or <i>ReportSummary.aspx</i>.
        /// </para>
        /// </remarks>
        public static UrlMappingItem CreateTemplatedMappingItem(string name, string templatedUrl, string redirection, IncomingQueryStringBehavior qsBehavior)
        {
            UrlMappingItem item = new UrlMappingItem(name, CreateTemplatedMappingRegex(templatedUrl, qsBehavior), redirection);
            return item;
        }

        /// <summary>
        /// Return a regular expression translated from the given templated URL.
        /// </summary>
        /// <param name="templatedUrl">A URL pattern relative to the application, with [TokenNames] included for dynamic matching</param>
        /// <param name="qsBehavior">defines how the UrlMappingModule should treat querystring values on an incoming URL for pattern matching; this is typically provided through declarative configuration</param>
        /// <returns>A regular expression as a <see cref="Regex" /> object</returns>
        /// <remarks>
        /// <para>
        /// A templated URL uses token names surrounded by [SquareBrackets] to provide for dynamic
        /// matching and parsing.
        /// For example, the URL template:
        /// <i>Reports/[ID]/[Action].aspx</i>
        /// translates to the following regular expression:
        /// <i>Reports/(?&lt;ID&gt;[a-zA-Z0-9_+%]*)/(?&lt;Action&gt;[a-zA-Z0-9_+%]*).aspx"</i>
        /// </para>
        /// <para>
        /// The regular expression is then used by the UrlMappingModule to match incoming URLs.
        /// If a match is found, token names and values are automatically appended to the 
        /// redirection URL as querystring arguments.  So, given the above templated URL string
        /// of <i>Reports/[ID]/[Action].aspx</i> with a Redirection defined as 
        /// <i>ReportAction.aspx</i>, an incoming URL of <i>Reports/47/Edit.aspx</i>
        /// is redirected by the UrlMappingModule to the following:
        /// <i>ReportAction.aspx?ID=47&amp;Action=Edit</i>.
        /// </para>
        /// </remarks>
        public static Regex CreateTemplatedMappingRegex(string templatedUrl, IncomingQueryStringBehavior qsBehavior)
        {
            // return a case-insensitive regex that must match the url completely
            return CreateTemplatedMappingRegex(templatedUrl, false, true, qsBehavior);
        }


        /// <summary>
        /// Return a regular expression translated from the given templated URL.
        /// </summary>
        /// <param name="templatedUrl">A URL pattern relative to the application, with [TokenNames] included for dynamic matching</param>
        /// <param name="caseSensitive"><i>true</i> if the returned expression should be case sensitive, <i>false</i> if not</param>
        /// <param name="matchCompletely"><i>true</i> if the returned expression should include beginning and ending anchors, ensuring that a match will require the complete URL and not a portion of it</param>
        /// <param name="qsBehavior">defines how the UrlMappingModule should treat querystring values on an incoming URL for pattern matching; this is typically provided through declarative configuration</param>
        /// <returns>A regular expression as a <see cref="Regex" /> object</returns>
        /// <remarks>
        /// <para>
        /// A templated URL uses token names surrounded by [SquareBrackets] to provide for dynamic
        /// matching and parsing.
        /// For example, the URL template:
        /// <i>Reports/[ID]/[Action].aspx</i>
        /// translates to the following regular expression:
        /// <i>Reports/(?&lt;ID&gt;[a-zA-Z0-9_+%]*)/(?&lt;Action&gt;[a-zA-Z0-9_+%]*).aspx"</i>
        /// </para>
        /// <para>
        /// The regular expression is then used by the UrlMappingModule to match incoming URLs.
        /// If a match is found, token names and values are automatically appended to the 
        /// redirection URL as querystring arguments.  So, given the above templated URL string
        /// of <i>Reports/[ID]/[Action].aspx</i> with a Redirection defined as 
        /// <i>ReportAction.aspx</i>, an incoming URL of <i>Reports/47/Edit.aspx</i>
        /// is redirected by the UrlMappingModule to the following:
        /// <i>ReportAction.aspx?ID=47&amp;Action=Edit</i>.
        /// </para>
        /// </remarks>
        public static Regex CreateTemplatedMappingRegex(string templatedUrl, bool caseSensitive, bool matchCompletely, IncomingQueryStringBehavior qsBehavior)
        {
            // return a regex that parses an app-relative url string and constructs an appropriate
            // Regular Expression for matching it; the template may include tokens in the form
            // of [TokenItem] which are matched as groups;
            // e.g. the url template "Reports/Display/[ID].aspx" becomes the regular expression:
            //   Reports/Display/(?<ID>[a-zA-Z0-9_+%]*).aspx"

            // if matchCompletely is true, the regex is anchored; if false, the match can occur
            // anywhere within the url

            // options for the returned regular expression
            RegexOptions options = (caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);

            // if the templatedUrl contains a querystring ? and we're incorporating querystring values in the regex
            //then escape it
            if (qsBehavior == IncomingQueryStringBehavior.Include)
                templatedUrl = templatedUrl.Replace("?", "\\?");

            // use a regex now to parse for [Tokens]
            Regex r = new Regex(@"(\[[a-zA-Z0-9_]*\])");
            MatchCollection matches = r.Matches(templatedUrl);
            string s = templatedUrl;
            foreach (Match m in matches)
            {
                string tokenName = m.Value.Substring(1, m.Value.Length - 2);
                string replacement = string.Format(@"(?<{0}>[\-?a-zA-Z0-9_\.\u00a4-\uffe5+]*)", tokenName);
                s = s.Replace(m.Value, replacement);
            }

            // if the matchCompletely option is desired, add anchors to the regex string
            if (matchCompletely)
                s = "^" + s + "$";

            // now construct and return the regular expression to test for these kinds of URLs
            return new Regex(s, options);
        }

    }
}
