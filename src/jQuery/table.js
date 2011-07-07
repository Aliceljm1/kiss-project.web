(function ($) {
    var settings;

    $.fn.gtable = function (opts) {
        var $this = $(this);
        $('>tbody tr', this).filter(':odd').addClass('odd');
        $('>tbody tr', this).hover(function () { $(this).addClass('hover'); }, function () { $(this).removeClass('hover'); });

        settings = $.extend(true, {}, $.fn.gtable.defaults, opts);

        var headerCheckbox;
        var columnCheckboxes;

        if (isNaN(settings.column)) {
            var sel = "thead tr th:" + settings.column + "-child input:checkbox,tfoot tr th:" + settings.column + "-child input:checkbox";
            headerCheckbox = $(sel, this);
            columnCheckboxes = $("tbody tr td:" + settings.column + "-child input:checkbox", this);
        }
        else {
            var sel = "thead tr th:nth-child(" + settings.column + ") input:checkbox,tfoot tr th:nth-child(" + settings.column + ") input:checkbox";
            headerCheckbox = $(sel, this);
            columnCheckboxes = $("tbody tr td:nth-child(" + settings.column + ") input:checkbox", this);
        }

        headerCheckbox.attr("title", settings.selectTip);

        headerCheckbox.click(function () {
            var checkedStatus = this.checked;

            columnCheckboxes.each(function () {
                this.checked = checkedStatus;
                if (this.checked)
                    $(this).parents('tr:first').addClass('selected');
                else
                    $(this).parents('tr:first').removeClass('selected');
            });

            if (checkedStatus == true) {
                $(this).attr("title", settings.unselectTip);
            }
            else {
                $(this).attr("title", settings.selectTip);
            }

            $this.trigger('gtable.row_selected', [$this.getSelectedRowIds()]);
        });

        if (headerCheckbox.attr('checked'))
            headerCheckbox.trigger('click');

        // click to select
        if (settings.clickToSelect) {
            $('tbody tr', this).filter(':has(:checkbox:checked)').addClass('selected').end().click(function (event) {
                if (event.target.type == 'checkbox')
                    $(this).toggleClass('selected');
                else if (event.target.tagName == 'TD') {
                    $(':checkbox:first', this).attr('checked', function () {
                        return !this.checked;
                    });
                    $(this).toggleClass('selected');
                }

                $this.trigger('gtable.row_selected', [$this.getSelectedRowIds()]);
            });
        }

        // sort
        var init_sort = function () {
            if (settings.sortablecolumns && typeof settings.sortablecolumns == 'string') {
                var columns = settings.sortablecolumns.split(',');

                jQuery.each(columns, function (i, v) {
                    var id = jQuery.trim(v);
                    if (id) $("thead tr [id='" + id + "']", $this).addClass('sortable');
                });
            }

            if (!$.query) return;

            var asc = true;
            var sort = jQuery.query.get('sort');
            if (sort && typeof sort == 'string') {
                asc = (sort.indexOf('-') == -1);
                if (!asc) sort = sort.substr(1);
                if (sort) $("thead [id='" + sort + "']", $this).addClass(asc ? 'asc' : 'desc');
            }

            $('thead .sortable', $this).click(function () {
                var column = $(this).attr('id');
                if (asc) column = '-' + column;
                var path = window.location.pathname;
                var index = path.indexOf('.');
                if (index != -1)
                    path = '1' + path.substr(index);

                window.location = path + jQuery.query.set('sort', column);
            });
        };

        init_sort();

        return $this;
    };

    $.fn.gtable.defaults = {
        column: 'first',
        selectTip: '全选',
        unselectTip: '全不选',
        clickToSelect: true,
        sortablecolumns: null
    };

    $.fn.getSelectedRowIds = function () {
        var ids = [];
        $.each($("tbody :checkbox[checked]", this), function (i, v) {
            ids.push(encodeURIComponent($(this).parents('tr:first').attr('id')));
        });
        return ids;
    };
    $.fn.removeRow = function (rowId) {
        var t = this;
        if (rowId.constructor.toString().indexOf("Array") == -1)
            $('tbody tr[id=' + decodeURIComponent(rowId) + ']', t).fadeOut(100, function () { $(this).remove(); });
        else {
            $.each(rowId, function (i, v) { $('tbody tr[id=' + decodeURIComponent(v) + ']', t).fadeOut(100, function () { $(this).remove(); }); });
        }
    };
})(jQuery);

String.prototype.startWith = function (str) {
    if (str == null || str == "" || this.length == 0 || str.length > this.length)
        return false;
    if (this.substr(0, str.length) == str)
        return true;
    else
        return false;
    return true;
};

(function ($) {
    $.fn.edit = function (opts) {
        var $this = $(this);

        opts = $.extend(true, {}, $.fn.edit.defaults, opts);

        if (typeof (gAjax) != 'undefined') {
            for (var m in gAjax) {
                if (opts.edit_func == null && m.startWith('edit'))
                    opts.edit_func = gAjax[m];

                if (opts.del_func == null && m.startWith('del'))
                    opts.del_func = gAjax[m];

                if (opts.save_func == null && m.startWith('save'))
                    opts.save_func = gAjax[m];
            }
        }

        var show_editor = function (id) {
            if (opts.edit_func != null)
                opts.edit_func.apply(null, [id, function (r) {
                    $('#dlg_edit').empty().html(r).gform({
                        submitFunc: function (formdata) {
                            opts.save_func.apply(null, [formdata, function (r) {
                                if (r <= 0 && opts.onerror && jQuery.isFunction(opts.onerror))
                                    opts.onerror.apply(null, [r]);
                                else {
                                    $('#dlg_edit').dialog('close');
                                    if (opts.sticky) {
                                        var q = window.location.search;
                                        if (!q)
                                            q = '?t=1';
                                        else {
                                            if (q.indexOf('t=0') != -1)
                                                q = q.replace('t=0', 't=1');
                                            else if (q.indexOf('t=1') != -1)
                                                q = q.replace('t=1', 't=0');
                                            else q = q + '&t=0';
                                        }
                                        window.location.href = window.location.pathname + q + "#" + id;
                                    } else {
                                        window.location.reload();
                                    }
                                }
                            } ]);
                        }
                    }).dialog('open');
                    if (opts.onDialogOpen && jQuery.isFunction(opts.onDialogOpen))
                        opts.onDialogOpen.apply(null, []);
                } ]);
        };

        $('.edit', $this).click(function () {
            show_editor(encodeURIComponent($(this).parents('tr:first').attr('id')));
            return false;
        });

        if (opts.dblclick) {
            $('tbody tr', $this).bind('dblclick', function () {
                $('.edit', $(this)).trigger('click');
                return false;
            });
        }

        var del_cb = function (ids, r) {
            if (opts.after_del_func && jQuery.isFunction(opts.after_del_func)) {
                if (opts.after_del_func.apply(null, [r]))
                    $this.removeRow(ids);
            }
            else
                $this.removeRow(ids);
        };

        var del_row = function () {
            if (opts.del_func != null && confirm(opts.del_confim_msg)) {
                var id = encodeURIComponent($(this).parents('tr:first').attr('id'));
                var ids = [];
                ids.push(id);
                opts.del_func.apply(null, [ids, function (r) { del_cb(id, r); } ]);
            };

            return false;
        };

        $('.delete', $this).click(del_row);

        $('.add').click(function () { show_editor(null); return false; });

        $('.batch_delete').click(function () {
            if (opts.del_func != null) {
                var ids = $this.getSelectedRowIds(true);
                if (ids.length == 0)
                    return false;
                if (confirm(opts.del_confim_msg))
                    opts.del_func.apply(null, [ids, function (r) { del_cb(ids, r); } ]);
            }
            return false;
        });

        if (window.location.hash && window.location.search.indexOf('t=') != -1) {
            $(':checkbox', window.location.hash).trigger('click');
        }

        return $this;
    };

    $.fn.edit.defaults = {
        edit_func: null,
        del_func: null,
        after_del_func: null,
        del_confim_msg: '确认？',
        save_func: null,
        onerror: null,
        onDialogOpen: null,
        dblclick: true,
        sticky: true
    };
})(jQuery);