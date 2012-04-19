/*
*JS文件动态加载工具
*/
var LazyInclude = {
    call: (function () {
        function hasFile(tag, url) {
            var contains = false;
            var type = (tag == "script") ? "src" : "href";

            $(tag + '[' + type + ']').each(function (i, v) {
                var attr = $(v).attr(type)
                if (attr == url || decodeURIComponent(attr).indexOf(url) != -1) {
                    contains = true;
                    return false;
                }
            });

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
                    var oScript = document.createElement('script');
                    oScript.type = 'text/javascript';
                    oScript.src = urls[i].url;
                    // most browsers
                    oScript.onload = urls[i].cb;
                    // IE 6 & 7
                    oScript.onreadystatechange = function () {
                        if (this.readyState == 'complete') {
                            urls[i].cb();
                        }
                    }
                    document.getElementsByTagName('head')[0].appendChild(oScript);
                }
                else {
                    urls[i].cb();
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