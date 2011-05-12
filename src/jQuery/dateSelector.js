Date.prototype.dateAdd = function(interval, number) {
    var d = this;
    var k = { 'y': 'FullYear', 'q': 'Month', 'm': 'Month', 'w': 'Date', 'd': 'Date', 'h': 'Hours', 'n': 'Minutes', 's': 'Seconds', 'ms': 'MilliSeconds' };
    var n = { 'q': 3, 'w': 7 };
    eval('d.set' + k[interval] + '(d.get' + k[interval] + '()+' + ((n[interval] || 1) * number) + ')');
    return d;
};
Date.prototype.toStr = function() {
    return this.getFullYear() + "-" + (this.getMonth() + 1) + "-" + this.getDate();
};
Date.prototype.dateDiff = function(interval, objDate2) {
    var d = this, i = {}, t = d.getTime(), t2 = objDate2.getTime();
    i['y'] = objDate2.getFullYear() - d.getFullYear();
    i['q'] = i['y'] * 4 + Math.floor(objDate2.getMonth() / 4) - Math.floor(d.getMonth() / 4);
    i['m'] = i['y'] * 12 + objDate2.getMonth() - d.getMonth();
    i['ms'] = objDate2.getTime() - d.getTime();
    i['w'] = Math.floor((t2 + 345600000) / (604800000)) - Math.floor((t + 345600000) / (604800000));
    i['d'] = Math.floor(t2 / 86400000) - Math.floor(t / 86400000);
    i['h'] = Math.floor(t2 / 3600000) - Math.floor(t / 3600000);
    i['n'] = Math.floor(t2 / 60000) - Math.floor(t / 60000);
    i['s'] = Math.floor(t2 / 1000) - Math.floor(t / 1000);
    return i[interval];
};

(function($) {
    $.fn.dateSelector = function(start, end) {
        var cal = Calendar.setup({
			onFocus: function(cal){
				$(cal.inputField).attr('lvalue', $(cal.inputField).val() );
			},
            onSelect: function(cal) { 
				if($(cal.inputField).attr('lvalue') == cal.selection.print(cal.dateFormat)[0])
					$(cal.inputField).val('');
				cal.hide(); 
			}
        });
        cal.manageFields(start, start, "%Y-%m-%d");
        cal.manageFields(end, end, "%Y-%m-%d");

        var input1 = $('#' + start);
        var input2 = $('#' + end);

        var $this = $(this);
        $this.change(function() {
            var day = parseInt($this.val(), 10);
            if (day > 0) {
                $this.next().hide();
                input1.val(Calendar.printDate(new Date().dateAdd('d', -day), "%Y-%m-%d"));
                input2.val(Calendar.printDate(new Date(),"%Y-%m-%d"));
            }
            else {
                $this.next().show();
            }
        }).trigger('change');
		
		if (input2.val() == Calendar.printDate(new Date(),"%Y-%m-%d")) {			
			var d1 = new Date(Date.parse(input1.val().replace(/-/g, "/")));
			var d2 = new Date(Date.parse(input2.val().replace(/-/g, "/")));
			var d = d1.dateDiff('d', d2);
			$this.val(d);
		}        
    };
})(jQuery);
