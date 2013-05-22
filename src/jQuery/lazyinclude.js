/*
*JS文件动态加载工具
*/
var LazyInclude = {
    scripts: [],
    loadScript: function (url, callback) {
        var script = this.scripts[url] || (this.scripts[url] = {
            loaded: false,
            callbacks: []
        });

        if (script.loaded) {
            return callback.apply();
        }

        script.callbacks.push({
            fn: callback
        });

        if (script.callbacks.length == 1) {
            $.ajax({
                type: 'GET',
                url: url,
                dataType: 'script',
                cache: true,
                success: function () {
                    script.loaded = true;
                    $.each(script.callbacks, function () {
                        this.fn.apply();
                    });
                    script.callbacks.length = 0;
                }
            });
        }
    },
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

            if (!$.isArray(files)) {
                return LazyInclude.loadScript(files.url, files.cb);
            }

            var counter = 0;

            // parallel loading
            return $.each(files, function (i, v) {
                LazyInclude.loadScript(v.url, v.cb);
            });
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

var lazy_embed = function (options) {
    window.lazy_embed_data = options;

    if ($('body').data('binded')) {
        $(window).hashchange();
        return;
    }

    lazy_include({
        jsFiles: [{
            url: (options.vp || '/') + '_res.aspx?r=alF1ZXJ5Lmhhc2guanM=&t=&z=1&v=1&su=1', cb: function () {

                $('body').data('binded', true);

                $(window).hashchange(function () {
                    var opts = window.lazy_embed_data;

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

                            if (opts.callback && jQuery.isFunction(opts.callback)) {
                                opts.callback.apply();
                            }
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