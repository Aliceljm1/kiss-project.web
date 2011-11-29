window.slidingCurtain = function (selector, startsOpen) {
    var $sc = $(selector);
    var closedHeight = 28;
    var openHeight;
    var path = $sc.attr('path');

    var curtain = {
        isOpen: function () {
            return $.cookie("cp_open") == "true";
        },
        open: function (e) {
            if (openHeight)
                $sc.height(openHeight);
            var openPos = { top: $sc.css('top'), left: "0px" };
            if (e) {
                $sc.animate(openPos);
            } else {
                $sc.css(openPos);
            }

            $sc.addClass("opened");
            $.cookie('cp_open', "true", { expires: 1, 'path': path });
        },
        close: function (e) {
            openHeight = $sc.height();
            var closedPos = { top: $sc.css('top'), left: (15 - $sc.width()) + "px" };
            if (e) {
                $sc.animate(closedPos);
            } else {
                $sc.css(closedPos);
            }
            $sc.removeClass("opened").height(closedHeight);
            $.cookie("cp_open", null, { 'path': path });
        }
    };

    if (startsOpen) {
        $sc.animate({ top: '0px', left: '0px' }).addClass("opened");
    } else if (curtain.isOpen()) {
        curtain.open();
    } else {
        curtain.close();
    }

    $sc.find(".close").click(curtain.close);
    $sc.find(".open").click(curtain.open);
};
$(function () {
    slidingCurtain('#_g_sc', false);
    $('#_g_sc').scrollFollow({ offset: 0 });
});