using System.Collections.Generic;

namespace Kiss.Web
{
    /// <summary>
    /// site info
    /// </summary>
    public interface ISite
    {
        string VirtualPath { get; }
        bool CombineJs { get; }
        bool CombineCss { get; }
        /// <summary>
        /// 默认主题
        /// </summary>
        string DefaultTheme { get; }
        string Authority { get; }
        string Title { get; }
        string RawAdditionalHeader { get; }

        string CssVersion { get; }
        string JsVersion { get; }

        string CssHost { get; }
        string JsHost { get; }

        string CssRoot { get; }
        string ThemeRoot { get; }

        string Host { get; }

        string FavIcon { get; }

        string ErrorPage { get; }

        string this[string key] { get; }

        List<DictSchema> GetSchema(string type);

        DictSchema GetSchema(string type, string name);

        string SiteKey { get; }

        /// <summary>
        /// 当前主题
        /// </summary>
        string Theme { get; set; }
    }
}
