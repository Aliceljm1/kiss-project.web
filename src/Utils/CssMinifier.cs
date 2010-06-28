using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Kiss.Web.Utils
{
    /// <summary>
    /// user this to minifier css
    /// </summary>
    [CLSCompliant(false)]
    public static class CssMinifier
    {
        public static int AppendReplacement(Match match, StringBuilder sb, string input, string replacement, int index)
        {
            var preceding = input.Substring(index, match.Index - index);

            sb.Append(preceding);
            sb.Append(replacement);

            return match.Index + match.Length;
        }

        public static void AppendTail(Match match, StringBuilder sb, string input, int index)
        {
            sb.Append(input.Substring(index));
        }

        public static uint ToUInt32(ValueType instance)
        {
            return Convert.ToUInt32(instance);
        }

        public static string RegexReplace(string input, string pattern, string replacement)
        {
            return Regex.Replace(input, pattern, replacement);
        }

        public static string RegexReplace(string input, string pattern, string replacement, RegexOptions options)
        {
            return Regex.Replace(input, pattern, replacement, options);
        }

        public static string Fill(string format, params object[] args)
        {
            return String.Format(format, args);
        }

        public static string RemoveRange(string input, int startIndex, int endIndex)
        {
            return input.Remove(startIndex, endIndex - startIndex);
        }

        public static bool EqualsIgnoreCase(string left, string right)
        {
            return String.Compare(left, right, true) == 0;
        }

        public static string ToHexString(int value)
        {
            var sb = new StringBuilder();
            var input = value.ToString();

            foreach (char digit in input)
            {
                sb.Append(Fill("{0:x2}", ToUInt32(digit)));
            }

            return sb.ToString();
        }

        #region YUI Compressor's CssMin originally written by Isaac Schlueter

        /// <summary>
        /// Minifies CSS.
        /// </summary>
        /// <param name="css">The CSS content to minify.</param>
        /// <returns>Minified CSS content.</returns>
        public static string CssMinify(string css)
        {
            return CssMinify(css, 0);
        }

        /// <summary>
        /// Minifies CSS with a column width maximum.
        /// </summary>
        /// <param name="css">The CSS content to minify.</param>
        /// <param name="columnWidth">The maximum column width.</param>
        /// <returns>Minified CSS content.</returns>
        public static string CssMinify(string css, int columnWidth)
        {
            css = RemoveCommentBlocks(css);
            css = RegexReplace(css, "\\s+", " ");
            css = RegexReplace(css, "\"\\\\\"}\\\\\"\"", "___PSEUDOCLASSBMH___");
            css = RemovePrecedingSpaces(css);
            css = RegexReplace(css, "([!{}:;>+\\(\\[,])\\s+", "$1");
            css = RegexReplace(css, "([^;\\}])}", "$1;}");
            css = RegexReplace(css, "([\\s:])(0)(px|em|%|in|cm|mm|pc|pt|ex)", "$1$2");
            css = RegexReplace(css, ":0 0 0 0;", ":0;");
            css = RegexReplace(css, ":0 0 0;", ":0;");
            css = RegexReplace(css, ":0 0;", ":0;");
            css = RegexReplace(css, "background-position:0;", "background-position:0 0;");
            css = RegexReplace(css, "(:|\\s)0+\\.(\\d+)", "$1.$2");
            css = ShortenRgbColors(css);
            css = ShortenHexColors(css);
            css = RegexReplace(css, "[^\\}]+\\{;\\}", "");

            if (columnWidth > 0)
            {
                css = BreakLines(css, columnWidth);
            }

            css = RegexReplace(css, "___PSEUDOCLASSBMH___", "\"\\\\\"}\\\\\"\"");
            css = css.Trim();

            return css;
        }

        private static string RemoveCommentBlocks(string input)
        {
            var startIndex = 0;
            var endIndex = 0;
            var iemac = false;

            startIndex = input.IndexOf(@"/*", startIndex);
            while (startIndex >= 0)
            {
                endIndex = input.IndexOf(@"*/", startIndex + 2);
                if (endIndex >= startIndex + 2)
                {
                    if (input[endIndex - 1] == '\\')
                    {
                        startIndex = endIndex + 2;
                        iemac = true;
                    }
                    else if (iemac)
                    {
                        startIndex = endIndex + 2;
                        iemac = false;
                    }
                    else
                    {
                        input = RemoveRange(input, startIndex, endIndex + 2);
                    }
                }
                startIndex = input.IndexOf(@"/*", startIndex);
            }

            return input;
        }

        private static string ShortenRgbColors(string css)
        {
            var sb = new StringBuilder();
            Regex p = new Regex("rgb\\s*\\(\\s*([0-9,\\s]+)\\s*\\)");
            Match m = p.Match(css);

            int index = 0;
            while (m.Success)
            {
                string[] colors = m.Groups[1].Value.Split(',');
                StringBuilder hexcolor = new StringBuilder("#");

                foreach (string color in colors)
                {
                    int val = Int32.Parse(color);
                    if (val < 16)
                    {
                        hexcolor.Append("0");
                    }
                    hexcolor.Append(ToHexString(val));
                }

                index = AppendReplacement(m, sb, css, hexcolor.ToString(), index);
                m = m.NextMatch();
            }

            AppendTail(m, sb, css, index);
            return sb.ToString();
        }

        private static string ShortenHexColors(string css)
        {
            var sb = new StringBuilder();
            Regex p = new Regex("([^\"'=\\s])(\\s*)#([0-9a-fA-F])([0-9a-fA-F])([0-9a-fA-F])([0-9a-fA-F])([0-9a-fA-F])([0-9a-fA-F])");
            Match m = p.Match(css);

            int index = 0;
            while (m.Success)
            {
                if (EqualsIgnoreCase(m.Groups[3].Value, m.Groups[4].Value) &&
                    EqualsIgnoreCase(m.Groups[5].Value, m.Groups[6].Value) &&
                    EqualsIgnoreCase(m.Groups[7].Value, m.Groups[8].Value))
                {
                    var replacement = String.Concat(m.Groups[1].Value, m.Groups[2].Value, "#", m.Groups[3].Value, m.Groups[5].Value, m.Groups[7].Value);
                    index = AppendReplacement(m, sb, css, replacement, index);
                }
                else
                {
                    index = AppendReplacement(m, sb, css, m.Value, index);
                }

                m = m.NextMatch();
            }

            AppendTail(m, sb, css, index);
            return sb.ToString();
        }

        private static string RemovePrecedingSpaces(string css)
        {
            var sb = new StringBuilder();
            Regex p = new Regex("(^|\\})(([^\\{:])+:)+([^\\{]*\\{)");
            Match m = p.Match(css);

            int index = 0;
            while (m.Success)
            {
                var s = m.Value;
                s = RegexReplace(s, ":", "___PSEUDOCLASSCOLON___");

                index = AppendReplacement(m, sb, css, s, index);
                m = m.NextMatch();
            }
            AppendTail(m, sb, css, index);

            var result = sb.ToString();
            result = RegexReplace(result, "\\s+([!{};:>+\\(\\)\\],])", "$1");
            result = RegexReplace(result, "___PSEUDOCLASSCOLON___", ":");

            return result;
        }

        private static string BreakLines(string css, int columnWidth)
        {
            int i = 0;
            int start = 0;

            var sb = new StringBuilder(css);
            while (i < sb.Length)
            {
                var c = sb[i++];
                if (c == '}' && i - start > columnWidth)
                {
                    sb.Insert(i, '\n');
                    start = i;
                }
            }
            return sb.ToString();
        }
        #endregion

    }
}
