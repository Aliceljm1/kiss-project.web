using System.Collections.Generic;

namespace Kiss.Web
{
    /// <summary>
    /// site info
    /// </summary>
    public interface ISite
    {
        string VirtualPath { get; }
        bool CombinJs { get; }
        bool CombinCss { get; }
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

        string UserKey { get; }
        string RoleKey { get; }

        string SiteKey { get; }
    }
}
