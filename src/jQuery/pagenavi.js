/*
支持键盘的分页
*/
(function ($) {
    $.fn.pageNavi = function (opts) {
        opts = $.extend(true, {}, $.fn.pageNavi.defaults, opts);

        var $this = $(this);

        var focusInInput = false;

        var navigation = function (event) {
            if (window.event) event = window.event;

            if (((opts.usectrlKey && event.ctrlKey) || !opts.usectrlKey) && !focusInInput) {
                var link = null;
                switch (event.keyCode ? event.keyCode : event.which ? event.which : null) {
                    case 0x25:
                        link = $(opts.prev, $this);
                        break;
                    case 0x27:
                        link = $(opts.next, $this);
                        break;
                }

                if (link != null) {
                    var href = link.attr('href');
                    if (href) document.location = href;
                }
            }
        };

        $(function () {
            $(':text,textarea,:file').live('focus', function () { focusInInput = true; }).live('blur', function () { focusInInput = false; });

            document.onkeydown = navigation;
        });

        return $this;
    };

    $.fn.pageNavi.defaults = {
        usectrlKey: false,
        next: '.next',
        prev: '.prev'
    };
})(jQuery);