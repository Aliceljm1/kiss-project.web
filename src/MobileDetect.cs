using Kiss.Utils;
using Kiss.Web.Utils;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace Kiss.Web
{
    public class MobileDetect
    {
        private static Regex MobileBrowsers =
            new Regex(@"android|avantgo|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\\/|plucker|pocket|psp|symbian|treo|up\\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private MobileDetect()
        {
        }

        public static MobileDetect Instance
        {
            get
            {
                return HttpContextUtil.GetAndSave<MobileDetect>("__mobiledetect__", () => { return new MobileDetect(); });
            }
        }

        private bool? _ismobile;
        public bool IsMobile
        {
            get
            {
                if (_ismobile == null)
                    _ismobile = MobileBrowsers.IsMatch(HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"]);

                return _ismobile.Value;
            }
        }

        public bool IsTablet
        {
            get { return false; }
        }

        /// <summary>
        /// 获取实际的主题名称。如果是移动端访问，主题名称后会加上后缀_mobile
        /// </summary>
        /// <param name="theme"></param>
        /// <returns></returns>
        public string GetRealThemeName(string theme)
        {
            if (IsMobile)
                return string.Format("{0}_mobile", theme);

            return theme;
        }

        public string GetRealThemeName(IArea area)
        {
            return GetRealThemeName(area, area.Theme);
        }

        public string GetRealThemeName(IArea area, string theme)
        {
            if (IsMobile && Directory.Exists(Path.Combine(ServerUtil.ResolveUrl(area.VirtualPath), "themes", string.Format("{0}_mobile", theme))))
                return string.Format("{0}_mobile", theme);

            return theme;
        }
    }
}
