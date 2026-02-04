


	jQuery("document").ready(function($){
    
    var nav = $('#menu');
    
    $(window).scroll(function () {
        if ($(this).scrollTop() > 103) {
            nav.addClass("f-nav animated slideInDown");
			
        } else {
            nav.removeClass("f-nav animated slideInDown");
        }
	
    });
	});
	


//// fire calender
//(function() {
//    $('.date input').on('click', function() {
//        $(this).closest('.date').find('.calendar-popup').slideToggle();
        
//    });
//}());

//// dropdown 
//(function() {
//    $('.dropdown-menu a').on('click', function(){    
//        $(this).closest('.dropdown').find('.dropdown-toggle').html($(this).html() + '<span class="caret"></span>');    
//    });
//}());

// nested table 
(function() {
   $('a[data-toggle="collapse"]').on('click', function(){
      $(this).closest('td').parent('tr').next('.showMore').slideToggle("slow"); 
   }); 
}());

// fire tooltip 
(function(){
    $('[data-toggle="tooltip"]').tooltip(); 
}());



// search 
(function() {
    $('.search-icon').on('click', function() {
       $('.task-search').fadeToggle("slow"); 
        
    });

}());
