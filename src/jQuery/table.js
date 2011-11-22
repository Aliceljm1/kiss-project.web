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
        else {
            columnCheckboxes.click(function () {
                if (this.checked)
                    $(this).parents('tr:first').addClass('selected');
                else
                    $(this).parents('tr:first').removeClass('selected');

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

            if (settings.sort == 'default' && !$.query) return;

            var asc = true;

            $('thead .sortable', $this).each(function () {
                var th = $(this),
                thIndex = th.index(),
                inverse = true;

                th.html('<div>' + th.html() + '<span></span></div>');

                var compare = th.attr('data_type') || 'string';

                $(this).click(function () {
                    if (settings.sort == 'inline') {
                        $this.last().find('td').filter(function () {
                            return $(this).index() === thIndex;
                        }).sortElements(function (a, b) {
                            var c;
                            if ($(a).attr('data_sort')) {
                                var d_a = $(a).attr('data_sort'), d_b = $(b).attr('data_sort');
                                if (compare == 'date')
                                    c = new Date(d_a) > new Date(d_b);
                                else if (compare == 'num')
                                    c = parseFloat(d_a) > parseFloat(d_b);
                                else
                                    c = d_a > d_b;
                            }
                            else
                                c = $.text([a]) > $.text([b]);
                            return c ?
                                inverse ? -1 : 1
                                : inverse ? 1 : -1;
                        }, function () {
                            return this.parentNode;
                        });

                        inverse = !inverse;

                        $('thead .sortable span', $this).removeClass('desc').removeClass('asc');

                        if (inverse) {
                            $(this).find('span').addClass('asc');
                        } else {
                            $(this).find('span').addClass('desc');
                        }
                    }
                    else if (settings.sort == 'default') {
                        var column = $(this).attr('id');
                        if (asc) column = '-' + column;
                        var path = window.location.pathname;
                        var index = path.indexOf('.');
                        if (index != -1)
                            path = '1' + path.substr(index);

                        window.location = path + jQuery.query.set('sort', column);
                    } else if (settings.sort == 'ajax') {
                        var column = $(this).attr('id');
                        if (asc) column = '-' + column;

                        var form = $this.parents('form:first');
                        if ($('input[name=sort]', form).length == 0)
                            form.append('<input type="hidden" name="sort"/>');

                        $('input[name=sort]', form).val(column);
                        $('input[name=page]', form).val(1);

                        form.submit();
                    }
                });
            });

            var sort = '';
            if (settings.sort == 'ajax') {
                var form = $this.parents('form:first');
                sort = $('input[name=sort]', form).val();
            }else if ($.query)
                sort = jQuery.query.get('sort');

            if (sort && typeof sort == 'string') {
                asc = (sort.indexOf('-') == -1);
                if (!asc) sort = sort.substr(1);
                if (sort) $("thead [id='" + sort + "'] span", $this).addClass(asc ? 'asc' : 'desc');
            }
        };

        init_sort();

        // column resize
        //get number of columns
        var numberOfColumns = $this.first().find('thead TR TH.resizable').size();
        if (numberOfColumns > 0) {

            var resetTableSizes = function (change, columnIndex) {
                //calculate new width       
                var myWidth = $this.first().find(' TR TH.resizable').get(columnIndex).offsetWidth;
                var newWidth = (myWidth + change);

                // resize th
                var th = $this.first().find('thead TR TH.resizable').eq(columnIndex);
                th.css('width', newWidth);

                var ix = $this.first().find('thead TR TH').index(th);

                $this.find(' TR').each(function () {
                    $(this).find('TD').eq(ix).css('width', newWidth);
                });
                resetSliderPositions();
            };

            var resetSliderPositions = function () {
                var h = $this.first().height();
                //put all sliders on the correct position
                $this.first().find('thead TR TH.resizable').each(function (index) {
                    var th = $(this);
                    var newSliderPosition = th.offset().left + th.outerWidth();
                    $this.first().parent().find('.draghandle:eq(' + index + ')').css({ left: newSliderPosition, height: h });
                });
            }

            for (var i = 0; i < numberOfColumns; i++) {
                $('<div class="draghandle"></div>').insertBefore($this.first()).data('ix', i).draggable({
                    axis: "x",
                    start: function () {
                        $(this).toggleClass("dragged");
                        //set the height of the draghandle to the current height of the table, to get the vertical ruler
                        $(this).css({ height: $this.last().height() + 'px' });
                    },
                    stop: function (event, ui) {
                        $(this).toggleClass("dragged");
                        var oldPos = ($(this).data("draggable").originalPosition.left);
                        var newPos = ui.position.left;
                        var index = $(this).data("ix");
                        resetTableSizes(newPos - oldPos, index);
                    }
                });
            };

            resetSliderPositions();

            $(window).bind('resize', function () {
                resetSliderPositions();
            });
        }
        return $this;
    };

    $.fn.gtable.defaults = {
        column: 'first',
        selectTip: '全选',
        unselectTip: '全不选',
        clickToSelect: true,
        sortablecolumns: null,
        sort: 'default'
    };

    $.fn.getSelectedRowIds = function () {
        var ids = [];
        $.each($("tbody :checkbox[checked]", this), function (i, v) {
            ids.push(encodeURIComponent($(this).parents('tr:first').attr('id')));
        });
        return ids;
    };
    $.fn.removeRow = function (rowId) {
        var redirect = function () {
            if ($('tbody tr', t).length == 0) {
                $.paging.goto('prev');
            }
            else {
                window.location.reload();
            }
        };

        var t = this;
        if (rowId.constructor.toString().indexOf("Array") == -1)
            $('tbody tr[id=' + decodeURIComponent(rowId) + ']', t).fadeOut(100, function () { $(this).remove(); redirect(); });
        else {
            $.each(rowId, function (i, v) { $('tbody tr[id=' + decodeURIComponent(v) + ']', t).fadeOut(100, function () { $(this).remove(); if (i == rowId.length - 1) redirect(); }); });
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

jQuery.fn.sortElements = (function () {

    var sort = [].sort;

    return function (comparator, getSortable) {

        getSortable = getSortable || function () { return this; };

        var placements = this.map(function () {

            var sortElement = getSortable.call(this),
                parentNode = sortElement.parentNode,

            // Since the element itself will change position, we have
            // to have some way of storing it's original position in
            // the DOM. The easiest way is to have a 'flag' node:
                nextSibling = parentNode.insertBefore(
                    document.createTextNode(''),
                    sortElement.nextSibling
                );

            return function () {

                if (parentNode === this) {
                    throw new Error(
                        "You can't sort elements if any one is a descendant of another."
                    );
                }

                // Insert before flag:
                parentNode.insertBefore(this, nextSibling);
                // Remove flag:
                parentNode.removeChild(nextSibling);

            };

        });

        return sort.call(this, comparator).each(function (i) {
            placements[i].call(getSortable.call(this));
        });

    };

})();

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


/*
分页js
1，支持键盘的分页
2，支持跳转到指定页码的api
*/
(function ($) {

    $.paging = {
        keyword: function () {
            var focusInInput = false;

            var navigation = function (event) {
                if (window.event) event = window.event;

                if (!focusInInput) {
                    switch (event.keyCode ? event.keyCode : event.which ? event.which : null) {
                        case 0x25:
                            $.paging.goto('prev');
                            break;
                        case 0x27:
                            $.paging.goto('next');
                            break;
                    }
                }
            };

            $(':text,textarea,:file').live('focus', function () { focusInInput = true; }).live('blur', function () { focusInInput = false; });

            document.onkeydown = navigation;
        },
        goto: function (p) {
            var path = window.location.pathname;
            var file = '';
            var pre = '';
            var ix_splash = path.lastIndexOf('/');
            if (ix_splash == -1)
                file = path;
            else {
                pre = path.substr(0, ix_splash + 1);
                file = path.substr(ix_splash + 1);
            }

            var page = '';
            var extension = '';
            var ix_dot = file.lastIndexOf('.');
            if (ix_dot == -1)
                page = file;
            else {
                page = file.substr(0, ix_dot);
                extension = file.substr(ix_dot);
            }

            var pi;
            if (!isNaN(parseInt(p, 10)))
                pi = parseInt(p, 10);
            else if (p == 'prev')
                pi = Math.max(1, parseInt(page) - 1);
            else if (p == 'next')
                pi = Math.min(1, parseInt(page) + 1);

            window.location = pre + pi + extension + window.location.search;
        },
        ajax: function () {
            var form = $('.pagination').parents('form:first');
            if (form.length == 0) return;

            $('.pagination a').click(function () {
                var p = 1;
                var ts = $(this);
                if (ts.hasClass('next'))
                    p = parseInt(ts.parents('.pagination:first').find('.current').text()) + 1;
                else if (ts.hasClass('prev'))
                    p = parseInt(ts.parents('.pagination:first').find('.current').text()) - 1;
                else
                    p = parseInt(ts.text());    
                    
                var cp = $('input[name=page]', form);
                if (cp.length == 0)
                    form.append('<input type="hidden" name="page"/>');                          

                $('input[name=page]', form).val(p);

                form.submit();

                return false;
            });            
        }
    };
})(jQuery);