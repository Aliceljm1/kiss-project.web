KISS Web

һ����������MVC��ܡ�������ASP.NET֮�ϣ���������WebForms��

���ԣ�
* ���������߼�
* �������������
* ��������
* ֧���첽����
* �����

�汾��ʷ��

��֪���⣺
�����������Ļ����£�Application_Start����Initִ��Ȩ�޵����⣨���޷�����IO�����������޷���¼��־��

Todo:
	�������֧���Ż�
	֧�ִ�ģ������Ĳ˵���

v2.1
ɾ����һЩControls�����ռ��µĹ�ʱ����
���㼯��Area��֧��ʹ��Ĭ��վ���ģ�壩

v2.0
����������Controller��list����ʹ��IRepository��GetDataTable��������ҪKiss1.6�汾���ϣ�
Scripts�ؼ�����JsMin�ű�
�޸�ajax�����£�Ȩ��ϵͳ�е�һ��bug
paging�ؼ��Ż�

v1.9
�Ƴ���ContextDataConfig��ͨ��ContextDataAttribute��ǩ��������������
�Ҳ���mvc������ʱ�׳�ControllerNotFoundEventArgs�¼�
�޸�ControlResolver��findAjaxMethods����Ϊpublic
�޸���asp.net viewstate��bug��ֻҪquerystring�ﺬ__VIEWSTATE��ϵͳ���쳣������viewstateҲ���У�
ͨ�������Ƴ���kissMasterFile querystring
Controller������hasMenu�������жϵ�ǰ�û��Ƿ���ָ���˵���Ȩ��
�Ż�css/js version�Ĵ���ʽ

v1.8
��Ҫ���£�ISite�ӿ��Ƴ���Id���ԣ�JContext����SiteId���ԣ�����վ��Ⱥ��Site������ʵ��ISite�ӿڣ�ֻ���𱣴�վ��Ⱥ�µ�������Ϣ
TemplatedControl֧�ִ�����skins��Ŀ¼ȡƤ��
Container�ؼ��Ż�
JContext���IsDesignMode����
һЩС���Ż�
�޸���UrlMappingModule��GetUrlRequested����������Ŀ¼·������С��ĸʱ��һ��bug
�޸���paging�ؼ���bug�����ٻ���js block
���ڴ�querystring��ȡtheme

v1.7
����Ĭ�ϴ���ҳ��http StatusCodeΪ500
վ��������Ż�themeRoot��cssRoot�����ã�virtualPath��siteKey���޸ģ�
��װsetupģ���ϵͳδ����ʱ�Զ���ת��setup
�޸���KissHttpApplication��bug��EventBroker.Instance.BeginRequest += onBeginRequest;����ε��ã�
�޸�Cache Provider��bug��IndexOutOfRange�쳣

v1.6
IUrlMappingProvider�����GetUrlsBySite����
JContext�����һЩ��������
Head�����meta��keywords��description��
�޸���һ��ControllerResolver��Զ�̬���򼯽�����һ��bug
֧��ͨ����ǩ���������������ִ������Ҫ��Ȩ��
������Ȩ��ģ��δ����ʱ�Ĵ����߼�
�滻��jsmin��ʵ�֣�����jsmin���쳣
��ֹ����Ӧ�ó����
�Ƴ���InitializerModule�������߼�����KissHttpApplication�վ����Ҫ���Global.asax�ļ����ļ�����<%@ Application Inherits="Kiss.Web.KissHttpApplication" %>

v1.5
���Ӷ�routes.config�в˵������ù���<menu url="~filemanager/index.aspx"/>
������4��ʵ���࣬site��menu��menuitem��url����sitegroupǨ�ƹ�����
IUrlMappingProvider�ӿ�����GetMenuItemsBySite����������վ�㷵��վ��Ĳ˵�
֧����վ�㲻ͬ�Ŀ�����
����ϵͳ�쳣ʱ�Ĵ�������
������KissHttpApplication�࣬����Ӧ�ó�������ʱ��һЩ��ʼ��
�޸���JavaScriptMinifier�������ռ�Ĵ���

v1.4
ȥ��webquery��LoadCondidtion������ȡpagesize�Ĵ���
�޸��˲˵�index��һ��bug
վ��·�����޸��ļ���ʱ������appdomain
�޸�����ajax������jcontext.userΪnull��bug
�޸���urlMappings.xml��url index��bug

v1.3
�Ƴ���ControllerContext��Ĺ�ʱ����
����֧��.net framework 2.0

v1.2
MVC����Controller������һЩCRUD�ı�������
Paging�ؼ�����SupportKeyboard����
ResourceCombineHandlerѹ���ϲ�js��css�Ż�
��¼��ϸ��mvc������������־

v1.1
����������vs2010
�����˶�mvcģ����쳣����,������ThreadAbortException
ɾ����Kiss.Web.Mvc.Controller��GetRepository����
JContext����UrlEncode����
���ƶ�����Ŀ¼��֧�֣��Զ�����Ϊ��ǰ����վ�������Ŀ¼
�޸���վ�㲿��������Ŀ¼�µķ�ҳ�ؼ�bug
�޸�Container�ؼ���Ĭ��ģ���ļ���bug
�Ż��˷�ҳ�ؼ���ajax�����µĹ���
�޸���ajax����Ϊnullʱ��bug
�޸��˺ϲ���Դhandler������Ŀ¼�����µ�bug( ���ؾ��⻷����δ�����ԣ�������bug��
�޸��˺ϲ�css��˳������

v1.0
���ӶԶ�����Ե�֧��
��¼�������������쳣
�����˶���־ϵͳ��hack
ҳ�����MasterPageĬ�ϲ�����master
ȡ��ResponseUtil��HTTP�����ĵ��������������첽������
����ͨ�������޸ĵ�ǰ��theme