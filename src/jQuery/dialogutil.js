; var dialogutil = function (id) {
    $('a[target="' + id + '"]').live('click', function () {
        var a = $(this);
        var ajax = a.attr('href');
        if (ajax && ajax.length > 1) {
            ajax = ajax.substr(1);

            var func = 'gAjax.';

            var i = ajax.indexOf('?');
            if (i != -1)
                func += ajax.substr(0, i);
            else
                func += ajax;

            if (!$.isFunction(eval(func))) {
                alert(func + ' is not a function!');
                return false;
            }

            func += '(';

            if (i > -1 && i != ajax.length - 1)
                func += (ajax.substr(i + 1) + ',');
            func += 'function (r) { if(r){$("' + id + '").html(r).dialog("open");} });';
            try {
                eval(func);
            } catch (e) {
            }
        }
        else {
            if ($(id).dialog('isOpen'))
                $(id).dialog('close');
            else
                $(id).dialog('open');
        }
        return false;
    });
};