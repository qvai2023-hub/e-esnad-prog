


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
	
 $(document).ready(function() {
	
	  $('#lightSlider').lightSlider({
        item:4,
        loop:true,
        autoWidth: false,
        slideMove:1,
        easing: 'cubic-bezier(0.25, 0, 0.25, 1)',
		auto: false,
        speed:800,
		rtl:true,
		pager: true,
		slideMargin:50,
		controls:false,
        responsive : [
            
            {
                breakpoint:992,
                settings: {
                    item:3,
                    slideMove:1,
                    slideMargin:60,
                  }
            },
            
            
             {
                breakpoint:702,
                settings: {
                    item:2,
                    slideMove:1,
                    slideMargin:80,
                  }
            },
            
            
            {
                breakpoint:480,
                settings: {
                    item:2,
                    slideMove:1,
		    slideMargin:40,
                  }
            },
            
             {
                breakpoint:430,
                settings: {
                    item:1,
                    slideMove:1,
		    slideMargin:40,
                  }
            }
            
            
        ]
    });
   



 $('#investSlider').lightSlider({
        item:1,
        loop:false,
        autoWidth: false,
        slideMove:1,
        easing: 'cubic-bezier(0.25, 0, 0.25, 1)',
		auto: true,
        speed:1000,
		rtl:true,
		pager: true,
		slideMargin:50,
		controls:false,
        responsive : [
            
            {
                breakpoint:992,
                settings: {
                    item:3,
                    slideMove:1,
                    slideMargin:30,
                  }
            },
            
            {
                breakpoint:768,
                settings: {
                    item:2,
                    slideMove:1,
                    slideMargin:30,
                  }
            },
             
             {
                breakpoint:600,
                settings: {
                    item:1,
                    slideMove:1,
                    slideMargin:30,
                  }
            },
           
        ]
    });

  });

// fire calender
(function() {
    $('.date input').on('click', function() {
        $(this).closest('.date').find('.calendar-popup').slideToggle();
        
    });
}());

// dropdown 
(function() {
    $('.dropdown-menu a').on('click', function(){    
        $(this).closest('.dropdown').find('.dropdown-toggle').html($(this).html() + '<span class="caret"></span>');    
    });
}());

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
