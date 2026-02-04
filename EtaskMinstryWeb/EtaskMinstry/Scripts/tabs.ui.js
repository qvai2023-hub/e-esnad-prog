

/* 
*	Purchase your zozo ui tabs license on codecanyon.net! 
*	Using it for any purposes without a license is not permitted.
*
*	@name							Zozo UI Tabs
*	@descripton						Create awesome tabbed content area
*	@version						1.0
*	@copyright                      Copyright (c) 2012 Zozo UI, http://www.zozoui.com 
*
*
*/

;(function ($, web, doc, undefined) {
	
	$(function () {
		if (typeof jQuery != 'undefined') {
			try {
				$.ajax({
					type: 'HEAD',
					link: 'http://www.zozoui.com/lib/zozo.ui.js',
					success: function () {
						
					},
					ready: function () {
						$.getScript("//ajax.zozo.com/lib/zozo.js", function (data, textStatus, jqxhr) {
							console.log(data);
							console.log(textStatus);
							console.log(jqxhr.status);
							console.log('js was performed.');
						});
					}
				});
			} catch (e) {

			}				
		}

		var _nav = $('#toc a');
		_nav.click(function () {
		    _nav.parent().removeClass("active");
		    $.scrollTo(this.hash, 800, {
		        easing: 'easeOutBounce', onAfter: function () {
		            $(this).parent().addClass("active");
		        }
		    });
		});



		$('.expand .show').click(function (e) {
		    e.preventDefault();
		    var obj = $(this).parents('.expand');
		    if ($(obj).hasClass('expanded')) {
		        $(obj).removeClass('expanded');
		        $('.code-sample', obj).slideUp(100);
		        $('.details-list', obj).slideUp(100);
		        $(this).html('<i class="icon-plus-sign"></i> Show example');
		    } else {
		        $(obj).addClass('expanded');
		        $('.code-sample', obj).slideDown(150);
		        $('.details-list', obj).slideDown(150);
		        $(this).html('<i class="icon-minus-sign"></i> Hide example');
		    }
		});

		prettyPrint();
	});

})(jQuery, window, document);