/*
*JS文件动态加载工具
*/
var LazyInclude = {
    call: (function () {
        function hasFile(tag, url) {
            var contains = false;
            var files = document.getElementsByTagName(tag);
            var type = tag == "script" ? "src" : "href";
            for (var i = 0, len = files.length; i < len; i++) {
                if (files[i].getAttribute(type) == url) {
                    contains = true;
                    break;
                }
            }
            return contains;
        }

        function loadCssFile(files) {
            var urls = files && typeof (files) == "string" ? [files] : files;

            for (var i = 0; i < urls.length; i++) {
                if (!hasFile("link", urls[i])) {
                    var ele = document.createElement("link");
                    ele.setAttribute('type', 'text/css');
                    ele.setAttribute('rel', 'stylesheet');
                    ele.setAttribute('href', urls[i]);

                    document.getElementsByTagName('head')[0].appendChild(ele);
                }
            }
        }

        function loadScript(files) {
            var urls = files && typeof (files) == "string" ? [files] : files;

            $.ajaxSetup({ cache: true });

            for (var i = 0; i < urls.length; i++) {
                if (!hasFile("script", urls[i].url)) {
                    $.getScript(urls[i].url, urls[i].cb);
                }
            }

            $.ajaxSetup({ cache: false });
        }

        function includeFile(options) {
            //首先加载css
            loadCssFile(options.cssFiles);
            //加载js                                 
            loadScript(options.jsFiles);
        }
        return { include: includeFile };
    })()
};

var lazy_include = function (options) {
    LazyInclude.call.include(options);
}