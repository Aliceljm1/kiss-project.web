
namespace Kiss.Web.UrlMapping
{
    /// <summary>
    /// url匹配失败时的动作类型
    /// </summary>
    public enum NoMatchAction
    {
        PassThrough = 0,
        ThrowException = 1,
        Return404 = 2,
        Redirect = 3
    }

    /// <summary>
    /// 定义里在应用程序的某个事件里处理url重定向
    /// </summary>
    public enum UrlProcessingEvent
    {        
        BeginRequest = 0,
        AuthenticateRequest = 1,
        AuthorizeRequest = 2
    }

    /// <summary>
    /// 定义了QueryString的行为
    /// </summary>
    public enum IncomingQueryStringBehavior
    {
        PassThrough = 0,
        Ingore = 1,
        Include = 2
    }
}
