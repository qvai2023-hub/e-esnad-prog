/* Arabic Translation for jQuery UI date picker plugin. */
/* Khaled Al Horani -- koko.dw@gmail.com */
/* خالد الحوراني -- koko.dw@gmail.com */
/* NOTE: monthNames are the original months names and they are the Arabic names, not the new months name فبراير - يناير and there isn't any Arabic roots for these months */
jQuery(function($){
	$.datepicker.regional['ar'] = {
		closeText: 'إغلاق',
		prevText: '&#x3c;السابق',
		nextText: 'التالي&#x3e;',
		currentText: 'اليوم',
		monthNames: ['يناير', 'فبراير', 'مارس', 'إبريل', 'مايو', 'يونيو',
		'يوليو', 'أغسطس', 'سبتمبر',	'أكتوبر', 'نوفمبر', 'ديسمبر'],
		monthNamesShort: ['1','2','3','4','5','6','7','8','9','10','11','12'],
		dayNames: [ 'الأحد', 'الاتنين', 'الثلاثاء', 'الاربعاء', 'الخميس', 'الجمعة','السبت'],
		dayNamesShort: [ 'أحد', 'اثنين', 'ثلاثاء', 'اربعاء', 'خميس', 'جمعه','سبت'],
		dayNamesMin: [ 'أحد', 'اثنين', 'ثلاثاء', 'اربعاء', 'خميس', 'جمعه','سبت'],
		dateFormat: 'dd/mm/yy', firstDay: 0,
		isRTL: true
	};
	$.datepicker.setDefaults($.datepicker.regional['ar']);                                                                                                                                                                                                                          
});
                                                                                                                                        