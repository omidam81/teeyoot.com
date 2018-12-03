// KEYHAN JavaScript Document
//if (document.location.pathname.startsWith("/Dashboard/")) $(".currency").hide();
$(window).load(function () { });

$(document).ready(function () {



    $('.navbtn').click(function () {
        $('nav').slideToggle();
        $('.menu-for-user').slideToggle();
        $('.tb-search-form input').toggleClass('menuclicked');
        $('.tb-search-form button').toggleClass('menuclicked2');
    });

    $('span.morecat').click(function () {
        $('ul.menu.menu-tags').slideToggle();
    });

    setInterval(function () {
        imageheight = ($('.bigbannerslider img').height());
        $('.bigbannerslider').css({ 'height': imageheight + 'px' });
    }, 1);
    if ($(".bigbannerslider").length > 0) {
        $(".bigbannerslider").responsiveslides({
            auto: true,
            pager: true,
            nav: true,
            speed: 500,
            timeout: 5500,
            namespace: "callbacks",
            before: function () {
                $('.events').append("<li>before event fired.</li>");
            },
            after: function () {
                $('.events').append("<li>after event fired.</li>");
            }
        });
    }

});
