/*!
* jQuery Form Plugin
* version: 2.43 (12-MAR-2010)
* @requires jQuery v1.3.2 or later
*
* Examples and documentation at: http://malsup.com/jquery/form/
* Dual licensed under the MIT and GPL licenses:
*   http://www.opensource.org/licenses/mit-license.php
*   http://www.gnu.org/licenses/gpl.html
*/
; (function ($) {

	/*
	Usage Note:
	-----------
	Do not use both ajaxSubmit and ajaxForm on the same form.  These
	functions are intended to be exclusive.  Use ajaxSubmit if you want
	to bind your own submit handler to the form.  For example,

	$(document).ready(function() {
	$('#myForm').bind('submit', function() {
	$(this).ajaxSubmit({
	target: '#output'
	});
	return false; // <-- important!
	});
	});

	Use ajaxForm when you want the plugin to manage all the event binding
	for you.  For example,

	$(document).ready(function() {
	$('#myForm').ajaxForm({
	target: '#output'
	});
	});

	When using ajaxForm, the ajaxSubmit function will be invoked for you
	at the appropriate time.
	*/

	/**
	* ajaxSubmit() provides a mechanism for immediately submitting
	* an HTML form using AJAX.
	*/
	$.fn.ajaxSubmit = function (options) {
		// fast fail if nothing selected (http://dev.jquery.com/ticket/2752)
		if (!this.length) {
			log('ajaxSubmit: skipping submit process - no element selected');
			return this;
		}

		if (typeof options == 'function')
			options = { success: options };

		var url = $.trim(this.attr('action'));
		if (url) {
			// clean url (don't include hash vaue)
			url = (url.match(/^([^#]+)/) || [])[1];
		}
		url = url || window.location.href || '';

		options = $.extend({
			url: url,
			type: this.attr('method') || 'GET',
			iframeSrc: /^https/i.test(window.location.href || '') ? 'javascript:false' : 'about:blank'
		}, options || {});

		// hook for manipulating the form data before it is extracted;
		// convenient for use with rich editors like tinyMCE or FCKEditor
		var veto = {};
		this.trigger('form-pre-serialize', [this, options, veto]);
		if (veto.veto) {
			log('ajaxSubmit: submit vetoed via form-pre-serialize trigger');
			return this;
		}

		// provide opportunity to alter form data before it is serialized
		if (options.beforeSerialize && options.beforeSerialize(this, options) === false) {
			log('ajaxSubmit: submit aborted via beforeSerialize callback');
			return this;
		}

		var a = this.formToArray(options.semantic);
		if (options.data) {
			options.extraData = options.data;
			for (var n in options.data) {
				if (options.data[n] instanceof Array) {
					for (var k in options.data[n])
						a.push({ name: n, value: options.data[n][k] });
				}
				else
					a.push({ name: n, value: options.data[n] });
			}
		}

		// give pre-submit callback an opportunity to abort the submit
		if (options.beforeSubmit && options.beforeSubmit(a, this, options) === false) {
			log('ajaxSubmit: submit aborted via beforeSubmit callback');
			return this;
		}

		// fire vetoable 'validate' event
		this.trigger('form-submit-validate', [a, this, options, veto]);
		if (veto.veto) {
			log('ajaxSubmit: submit vetoed via form-submit-validate trigger');
			return this;
		}

		var q = $.param(a);

		if (options.type.toUpperCase() == 'GET') {
			options.url += (options.url.indexOf('?') >= 0 ? '&' : '?') + q;
			options.data = null;  // data is null for 'get'
		}
		else
			options.data = q; // data is the query string for 'post'

		var $form = this, callbacks = [];
		if (options.resetForm) callbacks.push(function () { $form.resetForm(); });
		if (options.clearForm) callbacks.push(function () { $form.clearForm(); });

		// perform a load on the target only if dataType is not provided
		if (!options.dataType && options.target) {
			var oldSuccess = options.success || function () { };
			callbacks.push(function (data) {
				var fn = options.replaceTarget ? 'replaceWith' : 'html';
				$(options.target)[fn](data).each(oldSuccess, arguments);
			});
		}
		else if (options.success)
			callbacks.push(options.success);

		options.success = function (data, status, xhr) { // jQuery 1.4+ passes xhr as 3rd arg
			for (var i = 0, max = callbacks.length; i < max; i++)
				callbacks[i].apply(options, [data, status, xhr || $form, $form]);
		};

		// are there files to upload?
		var files = $('input:file', this).fieldValue();
		var found = false;
		for (var j = 0; j < files.length; j++)
			if (files[j])
				found = true;

		var multipart = false;
		//	var mp = 'multipart/form-data';
		//	multipart = ($form.attr('enctype') == mp || $form.attr('encoding') == mp);

		// options.iframe allows user to force iframe mode
		// 06-NOV-09: now defaulting to iframe mode if file input is detected
		if ((files.length && options.iframe !== false) || options.iframe || found || multipart) {
			// hack to fix Safari hang (thanks to Tim Molendijk for this)
			// see:  http://groups.google.com/group/jquery-dev/browse_thread/thread/36395b7ab510dd5d
			if (options.closeKeepAlive)
				$.get(options.closeKeepAlive, fileUpload);
			else
				fileUpload();
		}
		else
			$.ajax(options);

		// fire 'notify' event
		this.trigger('form-submit-notify', [this, options]);
		return this;


		// private function for handling file uploads (hat tip to YAHOO!)
		function fileUpload() {
			var form = $form[0];

			if ($(':input[name=submit]', form).length) {
				alert('Error: Form elements must not be named "submit".');
				return;
			}

			var opts = $.extend({}, $.ajaxSettings, options);
			var s = $.extend(true, {}, $.extend(true, {}, $.ajaxSettings), opts);

			var id = 'jqFormIO' + (new Date().getTime());
			var $io = $('<iframe id="' + id + '" name="' + id + '" src="' + opts.iframeSrc + '" onload="(jQuery(this).data(\'form-plugin-onload\'))()" />');
			var io = $io[0];

			$io.css({ position: 'absolute', top: '-1000px', left: '-1000px' });

			var xhr = { // mock object
				aborted: 0,
				responseText: null,
				responseXML: null,
				status: 0,
				statusText: 'n/a',
				getAllResponseHeaders: function () { },
				getResponseHeader: function () { },
				setRequestHeader: function () { },
				abort: function () {
					this.aborted = 1;
					$io.attr('src', opts.iframeSrc); // abort op in progress
				}
			};

			var g = opts.global;
			// trigger ajax global events so that activity/block indicators work like normal
			if (g && !$.active++) $.event.trigger("ajaxStart");
			if (g) $.event.trigger("ajaxSend", [xhr, opts]);

			if (s.beforeSend && s.beforeSend(xhr, s) === false) {
				s.global && $.active--;
				return;
			}
			if (xhr.aborted)
				return;

			var cbInvoked = false;
			var timedOut = 0;

			// add submitting element to data if we know it
			var sub = form.clk;
			if (sub) {
				var n = sub.name;
				if (n && !sub.disabled) {
					opts.extraData = opts.extraData || {};
					opts.extraData[n] = sub.value;
					if (sub.type == "image") {
						opts.extraData[n + '.x'] = form.clk_x;
						opts.extraData[n + '.y'] = form.clk_y;
					}
				}
			}

			// take a breath so that pending repaints get some cpu time before the upload starts
			function doSubmit() {
				// make sure form attrs are set
				var t = $form.attr('target'), a = $form.attr('action');

				// update form attrs in IE friendly way
				form.setAttribute('target', id);
				if (form.getAttribute('method') != 'POST')
					form.setAttribute('method', 'POST');
				if (form.getAttribute('action') != opts.url)
					form.setAttribute('action', opts.url);

				// ie borks in some cases when setting encoding
				if (!opts.skipEncodingOverride) {
					$form.attr({
						encoding: 'multipart/form-data',
						enctype: 'multipart/form-data'
					});
				}

				// support timout
				if (opts.timeout)
					setTimeout(function () { timedOut = true; cb(); }, opts.timeout);

				// add "extra" data to form if provided in options
				var extraInputs = [];
				try {
					if (opts.extraData)
						for (var n in opts.extraData)
							extraInputs.push(
							$('<input type="hidden" name="' + n + '" value="' + opts.extraData[n] + '" />')
								.appendTo(form)[0]);

					// add iframe to doc and submit the form
					$io.appendTo('body');
					$io.data('form-plugin-onload', cb);
					form.submit();
				}
				finally {
					// reset attrs and remove "extra" input elements
					form.setAttribute('action', a);
					t ? form.setAttribute('target', t) : $form.removeAttr('target');
					$(extraInputs).remove();
				}
			};

			if (opts.forceSync)
				doSubmit();
			else
				setTimeout(doSubmit, 10); // this lets dom updates render

			var domCheckCount = 100;

			function cb() {
				if (cbInvoked)
					return;

				var ok = true;
				try {
					if (timedOut) throw 'timeout';
					// extract the server response from the iframe
					var data, doc;

					doc = io.contentWindow ? io.contentWindow.document : io.contentDocument ? io.contentDocument : io.document;

					var isXml = opts.dataType == 'xml' || doc.XMLDocument || $.isXMLDoc(doc);
					log('isXml=' + isXml);
					if (!isXml && (doc.body == null || doc.body.innerHTML == '')) {
						if (--domCheckCount) {
							// in some browsers (Opera) the iframe DOM is not always traversable when
							// the onload callback fires, so we loop a bit to accommodate
							log('requeing onLoad callback, DOM not available');
							setTimeout(cb, 250);
							return;
						}
						log('Could not access iframe DOM after 100 tries.');
						return;
					}

					log('response detected');
					cbInvoked = true;
					xhr.responseText = doc.body ? doc.body.innerHTML : null;
					xhr.responseXML = doc.XMLDocument ? doc.XMLDocument : doc;
					xhr.getResponseHeader = function (header) {
						var headers = { 'content-type': opts.dataType };
						return headers[header];
					};

					if (opts.dataType == 'json' || opts.dataType == 'script') {
						// see if user embedded response in textarea
						var ta = doc.getElementsByTagName('textarea')[0];
						if (ta)
							xhr.responseText = ta.value;
						else {
							// account for browsers injecting pre around json response
							var pre = doc.getElementsByTagName('pre')[0];
							if (pre)
								xhr.responseText = pre.innerHTML;
						}
					}
					else if (opts.dataType == 'xml' && !xhr.responseXML && xhr.responseText != null) {
						xhr.responseXML = toXml(xhr.responseText);
					}
					data = $.httpData(xhr, opts.dataType);
				}
				catch (e) {
					log('error caught:', e);
					ok = false;
					xhr.error = e;
					$.handleError(opts, xhr, 'error', e);
				}

				// ordering of these callbacks/triggers is odd, but that's how $.ajax does it
				if (ok) {
					opts.success(data, 'success');
					if (g) $.event.trigger("ajaxSuccess", [xhr, opts]);
				}
				if (g) $.event.trigger("ajaxComplete", [xhr, opts]);
				if (g && ! --$.active) $.event.trigger("ajaxStop");
				if (opts.complete) opts.complete(xhr, ok ? 'success' : 'error');

				// clean up
				setTimeout(function () {
					$io.removeData('form-plugin-onload');
					$io.remove();
					xhr.responseXML = null;
				}, 100);
			};

			function toXml(s, doc) {
				if (window.ActiveXObject) {
					doc = new ActiveXObject('Microsoft.XMLDOM');
					doc.async = 'false';
					doc.loadXML(s);
				}
				else
					doc = (new DOMParser()).parseFromString(s, 'text/xml');
				return (doc && doc.documentElement && doc.documentElement.tagName != 'parsererror') ? doc : null;
			};
		};
	};

	/**
	* ajaxForm() provides a mechanism for fully automating form submission.
	*
	* The advantages of using this method instead of ajaxSubmit() are:
	*
	* 1: This method will include coordinates for <input type="image" /> elements (if the element
	*	is used to submit the form).
	* 2. This method will include the submit element's name/value data (for the element that was
	*	used to submit the form).
	* 3. This method binds the submit() method to the form for you.
	*
	* The options argument for ajaxForm works exactly as it does for ajaxSubmit.  ajaxForm merely
	* passes the options argument along after properly binding events for submit elements and
	* the form itself.
	*/
	$.fn.ajaxForm = function (options) {
		return this.ajaxFormUnbind().bind('submit.form-plugin', function (e) {
			e.preventDefault();
			$(this).ajaxSubmit(options);
		}).bind('click.form-plugin', function (e) {
			var target = e.target;
			var $el = $(target);
			if (!($el.is(":submit,input:image"))) {
				// is this a child element of the submit el?  (ex: a span within a button)
				var t = $el.closest(':submit');
				if (t.length == 0)
					return;
				target = t[0];
			}
			var form = this;
			form.clk = target;
			if (target.type == 'image') {
				if (e.offsetX != undefined) {
					form.clk_x = e.offsetX;
					form.clk_y = e.offsetY;
				} else if (typeof $.fn.offset == 'function') { // try to use dimensions plugin
					var offset = $el.offset();
					form.clk_x = e.pageX - offset.left;
					form.clk_y = e.pageY - offset.top;
				} else {
					form.clk_x = e.pageX - target.offsetLeft;
					form.clk_y = e.pageY - target.offsetTop;
				}
			}
			// clear form vars
			setTimeout(function () { form.clk = form.clk_x = form.clk_y = null; }, 100);
		});
	};

	// ajaxFormUnbind unbinds the event handlers that were bound by ajaxForm
	$.fn.ajaxFormUnbind = function () {
		return this.unbind('submit.form-plugin click.form-plugin');
	};

	/**
	* formToArray() gathers form element data into an array of objects that can
	* be passed to any of the following ajax functions: $.get, $.post, or load.
	* Each object in the array has both a 'name' and 'value' property.  An example of
	* an array for a simple login form might be:
	*
	* [ { name: 'username', value: 'jresig' }, { name: 'password', value: 'secret' } ]
	*
	* It is this array that is passed to pre-submit callback functions provided to the
	* ajaxSubmit() and ajaxForm() methods.
	*/
	$.fn.formToArray = function (semantic) {
		var a = [];
		if (this.length == 0) return a;

		var form = this[0];
		var els = semantic ? form.getElementsByTagName('*') : form.elements;
		if (!els) return a;
		for (var i = 0, max = els.length; i < max; i++) {
			var el = els[i];
			var n = el.name;
			if (!n) continue;

			if (semantic && form.clk && el.type == "image") {
				// handle image inputs on the fly when semantic == true
				if (!el.disabled && form.clk == el) {
					a.push({ name: n, value: $(el).val() });
					a.push({ name: n + '.x', value: form.clk_x }, { name: n + '.y', value: form.clk_y });
				}
				continue;
			}

			var v = $.fieldValue(el, true);
			if (v && v.constructor == Array) {
				for (var j = 0, jmax = v.length; j < jmax; j++)
					a.push({ name: n, value: v[j] });
			}
			else if (v !== null && typeof v != 'undefined')
				a.push({ name: n, value: v });
		}

		if (!semantic && form.clk) {
			// input type=='image' are not found in elements array! handle it here
			var $input = $(form.clk), input = $input[0], n = input.name;
			if (n && !input.disabled && input.type == 'image') {
				a.push({ name: n, value: $input.val() });
				a.push({ name: n + '.x', value: form.clk_x }, { name: n + '.y', value: form.clk_y });
			}
		}
		return a;
	};

	/**
	* Serializes form data into a 'submittable' string. This method will return a string
	* in the format: name1=value1&amp;name2=value2
	*/
	$.fn.formSerialize = function (semantic) {
		//hand off to jQuery.param for proper encoding
		return $.param(this.formToArray(semantic));
	};

	/**
	* Serializes all field elements in the jQuery object into a query string.
	* This method will return a string in the format: name1=value1&amp;name2=value2
	*/
	$.fn.fieldSerialize = function (successful) {
		var a = [];
		this.each(function () {
			var n = this.name;
			if (!n) return;
			var v = $.fieldValue(this, successful);
			if (v && v.constructor == Array) {
				for (var i = 0, max = v.length; i < max; i++)
					a.push({ name: n, value: v[i] });
			}
			else if (v !== null && typeof v != 'undefined')
				a.push({ name: this.name, value: v });
		});
		//hand off to jQuery.param for proper encoding
		return $.param(a);
	};

	/**
	* Returns the value(s) of the element in the matched set.  For example, consider the following form:
	*
	*  <form><fieldset>
	*	  <input name="A" type="text" />
	*	  <input name="A" type="text" />
	*	  <input name="B" type="checkbox" value="B1" />
	*	  <input name="B" type="checkbox" value="B2"/>
	*	  <input name="C" type="radio" value="C1" />
	*	  <input name="C" type="radio" value="C2" />
	*  </fieldset></form>
	*
	*  var v = $(':text').fieldValue();
	*  // if no values are entered into the text inputs
	*  v == ['','']
	*  // if values entered into the text inputs are 'foo' and 'bar'
	*  v == ['foo','bar']
	*
	*  var v = $(':checkbox').fieldValue();
	*  // if neither checkbox is checked
	*  v === undefined
	*  // if both checkboxes are checked
	*  v == ['B1', 'B2']
	*
	*  var v = $(':radio').fieldValue();
	*  // if neither radio is checked
	*  v === undefined
	*  // if first radio is checked
	*  v == ['C1']
	*
	* The successful argument controls whether or not the field element must be 'successful'
	* (per http://www.w3.org/TR/html4/interact/forms.html#successful-controls).
	* The default value of the successful argument is true.  If this value is false the value(s)
	* for each element is returned.
	*
	* Note: This method *always* returns an array.  If no valid value can be determined the
	*	   array will be empty, otherwise it will contain one or more values.
	*/
	$.fn.fieldValue = function (successful) {
		for (var val = [], i = 0, max = this.length; i < max; i++) {
			var el = this[i];
			var v = $.fieldValue(el, successful);
			if (v === null || typeof v == 'undefined' || (v.constructor == Array && !v.length))
				continue;
			v.constructor == Array ? $.merge(val, v) : val.push(v);
		}
		return val;
	};

	/**
	* Returns the value of the field element.
	*/
	$.fieldValue = function (el, successful) {
		var n = el.name, t = el.type, tag = el.tagName.toLowerCase();
		if (typeof successful == 'undefined') successful = true;

		if (successful && (!n || el.disabled || t == 'reset' || t == 'button' ||
		(t == 'checkbox' || t == 'radio') && !el.checked ||
		(t == 'submit' || t == 'image') && el.form && el.form.clk != el ||
		tag == 'select' && el.selectedIndex == -1))
			return null;

		if (tag == 'select') {
			var index = el.selectedIndex;
			if (index < 0) return null;
			var a = [], ops = el.options;
			var one = (t == 'select-one');
			var max = (one ? index + 1 : ops.length);
			for (var i = (one ? index : 0); i < max; i++) {
				var op = ops[i];
				if (op.selected) {
					var v = op.value;
					if (!v) // extra pain for IE...
						v = (op.attributes && op.attributes['value'] && !(op.attributes['value'].specified)) ? op.text : op.value;
					if (one) return v;
					a.push(v);
				}
			}
			return a;
		}
		return el.value;
	};

	/**
	* Clears the form data.  Takes the following actions on the form's input fields:
	*  - input text fields will have their 'value' property set to the empty string
	*  - select elements will have their 'selectedIndex' property set to -1
	*  - checkbox and radio inputs will have their 'checked' property set to false
	*  - inputs of type submit, button, reset, and hidden will *not* be effected
	*  - button elements will *not* be effected
	*/
	$.fn.clearForm = function () {
		return this.each(function () {
			$('input,select,textarea', this).clearFields();
		});
	};

	/**
	* Clears the selected form elements.
	*/
	$.fn.clearFields = $.fn.clearInputs = function () {
		return this.each(function () {
			var t = this.type, tag = this.tagName.toLowerCase();
			if (t == 'text' || t == 'password' || tag == 'textarea')
				this.value = '';
			else if (t == 'checkbox' || t == 'radio')
				this.checked = false;
			else if (tag == 'select')
				this.selectedIndex = -1;
		});
	};

	/**
	* Resets the form data.  Causes all form elements to be reset to their original value.
	*/
	$.fn.resetForm = function () {
		return this.each(function () {
			// guard against an input with the name of 'reset'
			// note that IE reports the reset function as an 'object'
			if (typeof this.reset == 'function' || (typeof this.reset == 'object' && !this.reset.nodeType))
				this.reset();
		});
	};

	/**
	* Enables or disables any matching elements.
	*/
	$.fn.enable = function (b) {
		if (b == undefined) b = true;
		return this.each(function () {
			this.disabled = !b;
		});
	};

	/**
	* Checks/unchecks any matching checkboxes or radio buttons and
	* selects/deselects and matching option elements.
	*/
	$.fn.selected = function (select) {
		if (select == undefined) select = true;
		return this.each(function () {
			var t = this.type;
			if (t == 'checkbox' || t == 'radio')
				this.checked = select;
			else if (this.tagName.toLowerCase() == 'option') {
				var $sel = $(this).parent('select');
				if (select && $sel[0] && $sel[0].type == 'select-one') {
					// deselect all other options
					$sel.find('option').selected(false);
				}
				this.selected = select;
			}
		});
	};

	// helper fn for console logging
	// set $.fn.ajaxSubmit.debug to true to enable debug logging
	function log() {
		if ($.fn.ajaxSubmit.debug) {
			var msg = '[jquery.form] ' + Array.prototype.join.call(arguments, '');
			if (window.console && window.console.log)
				window.console.log(msg);
			else if (window.opera && window.opera.postError)
				window.opera.postError(msg);
		}
	};

})(jQuery);

(function ($) {
	$.fn.gform = function (opts) {
		var $this = $(this);

		opts = $.extend(true, {}, $.fn.gform.defaults, opts);

		var bind = function () {
			var f = $this;
			$('select', f).addClass('ui-widget-content');
			$(':text,:password,textarea', f).addClass('ui-widget-content ui-corner-all');

			// default value
			$('select[selected]', f).each(function (i, v) {
				if( $(v).attr('multiple')=='multiple')
					$(v).val($(v).attr('selected').split(','));
				else
					$(v).val($(v).attr('selected'));
			}).trigger('change');

			window.setTimeout(function () {
				//$(':checkbox,:radio').removeAttr('checked');
				$(':checkbox[selected],:radio[selected]', f).each(function (i, v) {
					var $o = $(v);
					// if ($o.attr('selected') == $o.val())
					// $o.attr('checked', 'checked');
					$o.attr('checked', $o.attr('selected') == $o.val());
				});

				// focus first input
				if (!opts.bindOnly && opts.focus_first_input) {
					var inputs = $(':text:enabled:visible,textarea:enabled:visible', f).filter(function () {
						return !$(this).attr('readonly') && $(this).parent(':hidden').size() == 0;
					});
					if (inputs.length > 0)
						inputs[0].focus();
				}
			}, 100);

			if(opts.enter_to_submit){
				$('input:enabled', f).bind('keypress', function (e) {
					if (e.keyCode == 13) f.submit();
				});
			}

			$('.buttons :first,.submit', f).bind('click', function () {
				f.submit();
				return false;
			});

			if (opts.autoAddRedStar) {
				var inputs = $('input:enabled,textarea:enabled', f).filter(function () {
					return  $(this).attr('minlength') !== undefined && parseInt($(this).attr('minlength'),10)>0;
				});
				$.each(inputs, function (i, v) {
					var id = $(v).attr('id');
					if(id)
						$('label[for=' + id + ']', f).append('<span style="color: red;">*</span>');
				});
			}
		};

		bind();

		if (opts.bindOnly) return;

		var beforeSubmit = function (formData, jqForm, options) {
			if ($.fn.gform.working)
				return false;

			var valid = true;

			try {
				// clean up
				var elems = $(':input', $this).removeClass('ui-state-error');
				$.each(elems, function (i, v) {
					var ele = $(v);

					if (ele.parent(':hidden').size() > 0)
						return true;

					valid = checkLength(ele);
					if (valid) valid = checkValue(ele);
					if (valid) valid = checkClasses(ele);
					if (valid) valid = checkRegexp(ele);
					if (valid) valid = checkFunction(ele);

					if (!valid)
						return false;
				});

				if (valid && opts.submitFunc && $.isFunction(opts.submitFunc)) {
					//$.fn.gform.working = true;
					_tip.hide();
					var qs = $.param(formData);
					opts.submitFunc.apply(jqForm, [qs]);
					return false;
				}

				return valid;
			}
			catch (e) {
				$.fn.gform.working = false;
				alert(e.message || '出错了!');
				return false;
			};
		};

		var updateTips = function (t) {
			var tip;
			if ($.isFunction($().message))
				tip = $().message(t, gform.form);
			else
				tip = _tip.html(t);
			if (t) {
				if ($.isFunction(tip.effect))
					tip.effect("highlight", {}, 3000);
				else if (tip.length > 0)
					tip.show();
				else
					alert(t);
			}
			else
				tip.hide();
		};

        var handleException = function(result) {
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
                result = eval(exc.parameter);
            }
            return result;
        };

		var onSuccess = function (data, status) {
			$.fn.gform.working = false;
			var success = $('.success', $this);
			if (success.length == 0)
				_tip.hide();
			else
				updateTips(success.html());

            var r;
            try {
                r = jQuery.parseJSON(data);
            } catch (e) {
                r =  data;
            }
            if (r != null && r.__AjaxException) {
                r = handleException(result);
            }
			if (opts.onSuccess && $.isFunction(opts.onSuccess)){                
				opts.onSuccess.apply($this, [r, status]);
            }
		};

		var _getTitle = function (ele) {
			var title = ele.attr('key');
			if (!title)
				title = $('label[for=' + ele.attr('id') + ']', $this).text();
			if (!title)
				title = ele.prev().text();
			return $.trim(title).replace(':', '').replace('：', '').replace('*', '');
		};
		var checkLength = function (ele) {
			var minlen = parseInt(ele.attr('minlength'), 10);
			if (!isNaN(minlen) && minlen > -1) {
				var value = $.trim(ele.val().toString());
				if (value == '' || ele.val().toString() === ele.attr('title')) {
					ele.addClass('ui-state-error').focus();
					updateTips(_getTitle(ele) + "不能为空!");
					return false;
				}
				else if (value.length < minlen) {
					ele.addClass('ui-state-error').focus();
					updateTips(_getTitle(ele) + "的长度必须大于或等于" + minlen);
					return false;
				}
			}

			var maxlen = parseInt(ele.attr('maxlength'), 10);
			if (!isNaN(maxlen) && maxlen > -1) {
				var value = $.trim(ele.val().toString());
				if (value.length > maxlen) {
					ele.addClass('ui-state-error').focus();
					updateTips(_getTitle(ele) + "的长度不能超过" + maxlen);
					return false;
				}
			}

			return true;
		};
		var checkValue = function (ele) {
			if ( ele.val() == null || $.trim(ele.val().toString()) == '') return true;
			var min = parseFloat(ele.attr('min'));
			var max = parseFloat(ele.attr('max'));
			if (!isNaN(min) || !isNaN(max)) {
				var fv = parseFloat(ele.val().toString());
				if (isNaN(fv)) {
					ele.addClass('ui-state-error').focus();
					updateTips(_getTitle(ele) + '必须是一个数字！');
					return false;
				}
				else {
					if (fv <= min || fv >= max) {
						ele.addClass('ui-state-error').focus();
						updateTips(_getTitle(ele) + '必须大于' + min + ',小于' + max + '！');
						return false;
					}
				}
				return true;
			}

			return true;
		};
		var checkRegexp = function (o) {			
			var reg = o.attr('reg');
			if (!reg)
				return true;
			reg = eval(reg);
			if (!(reg.test($.trim(o.val().toString())))) {
				o.addClass('ui-state-error').focus();
				updateTips(_getTitle(o) + '的格式不正确!');
				return false;
			}
			return true;
		};
		var checkClasses = function (o) {			
			var reg = null;
			var cls = o.attr('class');
			if (!cls) return true;
            if ( o.val() == null || $.trim(o.val().toString()) == '') return true;
			$.each(cls.split(' '), function (i, v) {
				if (!v) return true;
				var r = $.fn.gform.commonregs[v];
				if (r == undefined) return true;
				reg = r;
				return false;
			});
			if (!reg) return true;
			if (!(reg.test($.trim(o.val().toString())))) {
				o.addClass('ui-state-error').focus();
				updateTips(_getTitle(o) + '的格式不正确!');
				return false;
			}
			return true;
		};
		var checkFunction = function (ele) {
			var func = ele.attr('func');
			if (!func)
				return true;
			func = eval(func);
			if ($.isFunction(func)) {
				var ret = func.apply(ele, [$.trim(ele.val().toString())]);
				if (!ret || !ret.ok) {
					ele.addClass('ui-state-error').focus();
					updateTips(ret.msg);
					return false;
				}
			}
			return true;
		};

		var _tip = $('.tip', $this);
		if (_tip.length == 0)
			_tip = $('.tip');
		if (opts.ajax) {
			if (!opts.url)
				opts.url = $this.attr('action') || window.location.toString();
            if(opts.url && opts.url.indexOf('#') != -1)
                opts.url = opts.url.substr(0, opts.url.indexOf('#'));
			if (opts.submitFunc == null && !opts.url) {
				alert('url不能为空'); return;
			}
			$this.ajaxForm({
				beforeSubmit: beforeSubmit,
				success: onSuccess,
				url: opts.url
			});
		} else {
			$this.submit(beforeSubmit);
		}

		return $this;
	};

	$.fn.gform.working = false;

	$.fn.gform.defaults = {
		bindOnly: false,
		ajax: true,
		url: '',
		onSuccess: null,
		submitFunc: null,
		autoAddRedStar: true,
		focus_first_input:true,
		enter_to_submit:true
	};

	$.fn.gform.commonregs = {
		ip: /^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$/,
		url: /^(https?|ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$/i,
		email: /^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+(\.[a-zA-Z0-9_-])+/,
		date: /^((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-8]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-))$/,
		num: /^[-]?\d+(\.\d+)?$/,
		mobile: /^[+]{0,1}(\d){1,3}[ ]?([-]?((\d)|[ ]){1,12})+$/,
		zipcode: /^[a-zA-Z0-9 ]{3,12}$/,
		phone: /^[+]{0,1}(\d){1,3}[ ]?([-]?((\d)|[ ]){1,12})+$/,
		password: /^(\w){6,20}$/,
		simpletext: /^[a-zA-Z][a-zA-Z0-9_]*$/,
		text: /^[^<^>^&]*$/
	};

	$.fn.resetform = function () {
		$(':input', this).removeClass('ui-state-error');
		this[0].reset();
	};

})(jQuery);

jQuery(document).ajaxStart(function () { jQuery('.gloading').show(); $.fn.gform.working = true; }).ajaxStop(function () { jQuery('.gloading').hide(); $.fn.gform.working = false; });