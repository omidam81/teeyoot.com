// KEYHAN JavaScript Document

$(window).load(function () { });

$(document).ready(function () {

	$('.turnback').click(function(){
		$('.front').css({'transform':'rotateY(90deg)'}, 300);
		setTimeout(function(){
			//$('.back').show();
			$('.back').css({'transform':'rotateY(0deg)'}, 300);
			//$('.front').hide();
		}, 300);
		$(this).css({'opacity':'1'});
		$('.turnfront').css({'opacity':'0.5'});
	});
	$('.turnfront').click(function(){
		$('.back').css({'transform':'rotateY(-90deg)'}, 300);
		setTimeout(function(){
			//$('.front').show();
			$('.front').css({'transform':'rotateY(0deg)'}, 300);
			//$('.back').hide();
		}, 300);
		$(this).css({'opacity':'1'});
		$('.turnback').css({'opacity':'0.5'});
	});

	$('.colors ul li').click(function(){
		$('.colors ul li').removeClass('active');
		$(this).addClass('active');
	});

	$('.navbtn').click(function(){
		$('nav').slideToggle();
	});

	$('#zoomlensfront').elevateZoom();
	
	//$('#zoomlensback').elevateZoom();

});