window.ConvertPirce = function (s, c) {
   s = Number(s);
    if (c == window.Currency) return s.toFixed(2);
    var cs = 1;
    for (var i = 0; i < window.ExchangRate.length; i++) {
        var element = window.ExchangRate[i];
        if (element.From == c && element.To == window.Currency) cs = element.RateForBuyer;

    }
    return (s * cs).toFixed(2);
};

window.ConvertPirce2 = function (s, c1, c2) {
    s = Number(s);
    if (c1 == c2) return s;
    var cs = 1;
    for (var i = 0; i < window.ExchangRate.length; i++) {
        var element = window.ExchangRate[i];
        if (element.From == c1 && element.To == c2) cs = element.RateForBuyer;

    }
    return (s * cs);
};




window.createCookie = function (name, value, days) {
    var expires;
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toGMTString();
    }
    else {
        expires = "";
    }
    document.cookie = name + "=" + value + expires + "; path=/";
};

window.getCookie = function getCookie(c_name) {
    if (document.cookie.length > 0) {
        c_start = document.cookie.indexOf(c_name + "=");
        if (c_start != -1) {
            c_start = c_start + c_name.length + 1;
            c_end = document.cookie.indexOf(";", c_start);
            if (c_end == -1) {
                c_end = document.cookie.length;
            }
            return unescape(document.cookie.substring(c_start, c_end));
        }
    }
    return "";
};

var select_element = document.querySelector('.currencies');
lang = window.getCookie("selected-language");
if (lang) {
    $(select_element).val(lang);
} else {
    lang = window.countryCurreny;
    $(select_element).val(lang);
}


$(document).ready(function () {
    
   

    $(select_element).change(function () {

        var elem = (typeof this.selectedIndex === "undefined" ? window.event.srcElement : this);
        var value = elem.value || elem.options[elem.selectedIndex].value;
        //alert(value);
        window.Currency = value;
        //$(".price-currency-code").addClass('hidden').removeClass('shown').hide();
        //$(".price-" + value).addClass('shown').removeClass('hidden').show();
        //if (window.AjaxRefresh) {
        window.createCookie("selected-language", window.Currency);







        prices = $(".price-currency-code");
        if (!prices.length) return;

        $(prices).each(function (index, price) {





            var bc = $(price).data("bc");
            var bp = $(price).data("price");
            var format = $(price).data("string-format");

            var newPrice = window.ConvertPirce(bp, bc);

            newText = String.format(format, window.Currency + '' + newPrice);

            $(price).text(newText);

        });

        //var elem = (typeof this.selectedIndex === "undefined" ? window.event.srcElement : this);
        //var value = elem.value || elem.options[elem.selectedIndex].value;
        //old_currency = window.Currency;
        //window.Currency = value;
        //$(".price-currency-code").addClass('hidden').removeClass('shown').hide();
        //$(".price-" + value).addClass('shown').removeClass('hidden').show();
        //window.createCookie("selected-language", window.Currency);
        //$(".products option").not(".hidden").first().attr('selected', 'selected');

    });

    $(select_element).change();
});

