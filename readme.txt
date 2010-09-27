KISS Web

一个轻量级的MVC框架。构建在ASP.NET之上，但抛弃了WebForms和控件。

特性：
* 分离界面和逻辑
* 代码简洁易于理解
* 无需配置
* 支持异步操作
* 部署简单

版本历史：

v2.6.1
增加对多国语言的支持
记录控制器方法的异常
增加了对日志系统的hack
页面积累MasterPage默认不加载master
取消ResponseUtil对HTTP上下文的依赖（可用于异步方法）
可以通过代码修改当前的theme

v2.6.2
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

v2.6.3
MVC基类Controller增加了一些CRUD的便利方法
Paging控件增加SupportKeyboard属性
ResourceCombineHandler压缩合并js，css优化
记录详细的mvc控制器加载日志

v2.6.4
移除了ControllerContext里的过时代码
不在支持.net framework 2.0

v2.6.5
去除webquery的LoadCondidtion方法里取pagesize的代码
修复了菜单index的一个bug
站点路径下修改文件夹时不重启appdomain
修复了在ajax环境下jcontext.user为null的bug
修复了urlMappings.xml里url index的bug

v2.6.6
增加对routes.config中菜单的设置规则，<menu url="~filemanager/index.aspx"/>
增加了4个实体类，site，menu，menuitem，url（从sitegroup迁移过来）
IUrlMappingProvider接口增加GetMenuItemsBySite方法，根据站点返回站点的菜单
支持子站点不同的控制器
增加系统异常时的错误链接
增加了KissHttpApplication类，用于应用程序启动时的一些初始化
修复了JavaScriptMinifier类命名空间的错误

v2.6.7
IUrlMappingProvider添加了GetUrlsBySite方法
JContext添加了一些便利方法
Head里输出meta（keywords，description）

	Todo:
	多国语言支持优化