KISS Web

一个轻量级的MVC框架。构建在ASP.NET之上，但抛弃了WebForms。

特性：
* 分离界面和逻辑
* 代码简洁易于理解
* 无需配置
* 支持异步操作
* 部署简单

版本历史：

已知问题：
在虚拟主机的环境下，Application_Start，和Init执行权限的问题（因无法进行IO操作，导致无法记录日志）

Todo:
	多国语言支持优化
	支持带模板参数的菜单项

v2.1
删除了一些Controls命名空间下的过时代码
方便集成Area（支持使用默认站点的模板）

v2.0
控制器基类Controller的list方法使用IRepository的GetDataTable方法（需要Kiss1.6版本以上）
Scripts控件不再JsMin脚本
修复ajax环境下，权限系统中的一个bug
paging控件优化

v1.9
移除了ContextDataConfig，通过ContextDataAttribute标签定义上下文数据
找不到mvc控制器时抛出ControllerNotFoundEventArgs事件
修改ControlResolver的findAjaxMethods方法为public
修复了asp.net viewstate的bug（只要querystring里含__VIEWSTATE，系统就异常，禁用viewstate也不行）
通过反射移除了kissMasterFile querystring
Controller类增加hasMenu方法，判断当前用户是否有指定菜单的权限
优化css/js version的处理方式

v1.8
重要更新，ISite接口移除了Id属性，JContext增加SiteId属性，用于站点群；Site对象不在实现ISite接口，只负责保存站点群下的配置信息
TemplatedControl支持从主题skins跟目录取皮肤
Container控件优化
JContext添加IsDesignMode属性
一些小的优化
修复了UrlMappingModule类GetUrlRequested方法在虚拟目录路径含大小字母时的一个bug
修复了paging控件的bug，不再缓存js block
不在从querystring里取theme

v1.7
设置默认错误页面http StatusCode为500
站点设置里，优化themeRoot和cssRoot的配置；virtualPath和siteKey可修改；
安装setup模块后，系统未配置时自动跳转到setup
修复了KissHttpApplication的bug（EventBroker.Instance.BeginRequest += onBeginRequest;被多次调用）
修复Cache Provider的bug，IndexOutOfRange异常

v1.6
IUrlMappingProvider添加了GetUrlsBySite方法
JContext添加了一些便利方法
Head里输出meta（keywords，description）
修复了一个ControllerResolver里对动态程序集解析的一个bug
支持通过标签定义控制器方法的执行所需要的权限
增加了权限模块未设置时的处理逻辑
替换了jsmin的实现，忽略jsmin的异常
防止回收应用程序池
移除了InitializerModule，处理逻辑放在KissHttpApplication里，站点需要添加Global.asax文件，文件内容<%@ Application Inherits="Kiss.Web.KissHttpApplication" %>

v1.5
增加对routes.config中菜单的设置规则，<menu url="~filemanager/index.aspx"/>
增加了4个实体类，site，menu，menuitem，url（从sitegroup迁移过来）
IUrlMappingProvider接口增加GetMenuItemsBySite方法，根据站点返回站点的菜单
支持子站点不同的控制器
增加系统异常时的错误链接
增加了KissHttpApplication类，用于应用程序启动时的一些初始化
修复了JavaScriptMinifier类命名空间的错误

v1.4
去除webquery的LoadCondidtion方法里取pagesize的代码
修复了菜单index的一个bug
站点路径下修改文件夹时不重启appdomain
修复了在ajax环境下jcontext.user为null的bug
修复了urlMappings.xml里url index的bug

v1.3
移除了ControllerContext里的过时代码
不在支持.net framework 2.0

v1.2
MVC基类Controller增加了一些CRUD的便利方法
Paging控件增加SupportKeyboard属性
ResourceCombineHandler压缩合并js，css优化
记录详细的mvc控制器加载日志

v1.1
工程升级到vs2010
增加了对mvc模块的异常处理,忽略了ThreadAbortException
删除了Kiss.Web.Mvc.Controller的GetRepository方法
JContext增加UrlEncode方法
完善对虚拟目录的支持，自动设置为当前部署站点的虚拟目录
修复了站点部署在虚拟目录下的分页控件bug
修复Container控件的默认模板文件的bug
优化了分页控件在ajax环境下的功能
修复了ajax参数为null时的bug
修复了合并资源handler在虚拟目录环境下的bug( 负载均衡环境下未做测试，可能有bug）
修复了合并css的顺序问题

v1.0
增加对多国语言的支持
记录控制器方法的异常
增加了对日志系统的hack
页面积累MasterPage默认不加载master
取消ResponseUtil对HTTP上下文的依赖（可用于异步方法）
可以通过代码修改当前的theme