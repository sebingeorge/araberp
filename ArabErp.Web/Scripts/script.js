$(document).ready(function() {
		$menuLeft = $('.pushmenu-left');
		$nav_list = $('#nav_list');
		
		$nav_list.click(function() {
			$(this).toggleClass('active');
			$('.pushmenu-push').toggleClass('pushmenu-push-toright');
			$menuLeft.toggleClass('pushmenu-open');
			
		});
		$('#containerId').click(function () {
		    
		    $(this).removeClass('active');
		    $('.pushmenu-push').removeClass('pushmenu-push-toright');
		    $menuLeft.removeClass('pushmenu-open');
		});
		$('#NavBarDivId').click(function () {

		    $(this).removeClass('active');
		    $('.pushmenu-push').removeClass('pushmenu-push-toright');
		    $menuLeft.removeClass('pushmenu-open');
		});
		
	});