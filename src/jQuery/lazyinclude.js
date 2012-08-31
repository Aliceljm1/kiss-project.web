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

                    // store callback function
                    oScript.tmp = urls[i].cb;
                    oScript.onload = oScript.onreadystatechange = function () {
                        if (!this.readyState || /loaded|complete/.test(this.readyState)) {
                            this.tmp();

                            this.onload = this.onreadystatechange = this.tmp = null;
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

        function includeFile(opts) {
            //首先加载css
            loadCssFile(opts.cssFiles);
            //加载js                                 
            loadScript(opts.jsFiles);
        }
        return { include: includeFile };
    })()
};

var lazy_include = function (opts) {
    LazyInclude.call.include(opts);
};

function handleException(result) {
    if (result == null || !result.__AjaxException)
        return result;

    var exc = result.__AjaxException;
    if (exc.action == "JSMethod") {
        result = null;
        eval(exc.parameter + "()");
    }
    else if (exc.action == "JSEval") {
        result = null;
        eval(exc.parameter);
    }
    else if (exc.action == "returnValue") {
        return exc.parameter;
    }
};

var lazy_embed = function (opts) {

    lazy_include({ jsFiles: [{ url: '_res.aspx?r=alF1ZXJ5Lmhhc2guanM=&t=&z=1&v=1&su=1', cb: function () {
        $(window).hashchange(function () {
            var url = opts.url;
            if (window.location.hash) {
                url = window.location.hash.substr(1);

                if (url.indexOf('?') == 0) {
                    if (opts.url.indexOf('?') == -1)
                        url = opts.url + url;
                    else
                        url = opts.url + '&' + url.substr(1);
                }
            }

            jQuery.ajax({
                headers: { 'embed': '1', 'embedUrl': (opts.affect_url || '') },
                dataType: 'json',
                url: url,
                success: function (data) {
                    data = handleException(data);

                    opts.container.empty().html(data);
                },
                error: function () { }
            });
        });

        $(window).hashchange();
    }
    }],
        cssFiles: []
    });
};