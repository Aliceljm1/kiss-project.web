; var dialogutil = function (id) {
    $('a[href^="' + id + '"]').live('click', function () {
        var a = $(this);
        var ajax = a.attr('href').substr(id.length);
        if (ajax) {
            ajax = ajax.substr(1);
            var i = ajax.indexOf('?');
            var func = ajax.substr(0, i) + '(';
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