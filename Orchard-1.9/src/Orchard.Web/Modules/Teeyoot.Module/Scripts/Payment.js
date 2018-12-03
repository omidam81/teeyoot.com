var Payment = function() {

    // ReSharper disable InconsistentNaming
    var _sellerCountry = $("#seller_country_id").val();
    var _country = $("#hidden_country").val();
    var _delivery = new BigNumber(0);
    var _total = new BigNumber($("#ordTotal").data("price"));
    // ReSharper restore InconsistentNaming

    var setDelivery = function(delivery) {
        if (!delivery) {
            _delivery = new BigNumber(0);
        } else {
            if (typeof (delivery) == "string") {
                dd = parseFloat(delivery);
            }
            else {
                dd = parseFloat(delivery.toFixed(5));
            }
            
            dd = window.ConvertPirce(dd, window.Currency);

            _delivery = new BigNumber(dd);
        }
    };

    var getExchangeRate = function(country) {
        var exchangeRateVal = $("#Country option[value=\"" + country + "\"]").data("exchange-rate");
        var exchangeRate = new BigNumber(exchangeRateVal);

        return exchangeRate;
    };

    var convertPrice = function(price, fromExchangeRate, toExchangeRate) {
        var convertedPrice = price.dividedBy(fromExchangeRate).times(toExchangeRate);

        return convertedPrice;
    };

    var fillStatesControl = function(settings) {

        var selectedItem = $("#State").val();


        $("#State option:not(:first)").remove();



        var stateOptionHtml;

        for (var i = 0; i < settings.length; i++) {
            if (settings[i].Enabled) {
                stateOptionHtml = "<option value=\"%STATEVALUE%\" data-time-to-deliver=\"%DELIVERYTIME%\"  data-delivery-cost=\"%DELIVERYCOST%\">%STATENAME%</option>"
                    .replace("%STATEVALUE%", settings[i].State)
                    .replace("%DELIVERYCOST%", settings[i].DeliveryCost)
                    .replace("%STATENAME%", settings[i].State)
                    .replace("%DELIVERYTIME%", settings[i].DeliveryTime);

                $("#State").append(stateOptionHtml);
            }
        }

        $("#State").val(selectedItem);
        $("#State").change();
    };

    var getDeliverySettings = function(fromCountryId, toCountryId, cashOnDelivery) {

        var url = "/GetSettings?countryFromId=" + fromCountryId + "&countryToId=" + toCountryId + "&orderId=" + window.order_id;
        if (cashOnDelivery === true) {
            url += "&cashOnDelivery=true";
        }

        var deferred = $.ajax({
            cache: false,
            type: "GET",
            url: url,
            error: function(data) {
                alert("Status: " + data.status + ". Error message: " + data.statusText);
            }
        }).pipe(function(data) {
            return data.settings;
        });

        return deferred.promise();
    };

    var refreshPrices = function(country) {
        $("#deliveryCost").data('price', _delivery.toFixed(10)).html(_delivery.toFixed(2));
        $("#parentDelivery").data('price', _delivery.toFixed(10)).html(_delivery.toFixed(2));
        $("#ordTotal").data('price', _total.toFixed(10)).html(_total.toFixed(2));


        $(".currencies").change();
        //var currencyCode = $("#Country option[value=\"" + country + "\"]").data("currency-code");
        //$(".currency-code-placeholder").html(currencyCode);
    };

    var convertPrices = function(fromCountry, toCountry) {

        //var fromExchangeRate = getExchangeRate(fromCountry);
        //var toExchangeRate = getExchangeRate(toCountry);

        //$(".item-price-value-placeholder").each(function(index, itemPriceElement) {
        //    var priceVal = $(itemPriceElement).html();
        //    var price = new BigNumber(priceVal);
        //    var convertedPrice = convertPrice(price, fromExchangeRate, toExchangeRate);

        //    $(itemPriceElement).html(convertedPrice.toFixed(2));
        //});

        //_total = _total; //convertPrice(_total, fromExchangeRate, toExchangeRate);
    };

    var changeCountryTo = function(country) {

        var cashOnDelivery = false;

        var selectedPaymentMethod = $("#paymentMethod").val();
        if (selectedPaymentMethod === "4") {
            cashOnDelivery = true;
        }

        getDeliverySettings(_sellerCountry, country, cashOnDelivery).done(function (data) {
            x = Number($("#Order_TotalSold").val());

            $(data).each(function (index, item) {
                var percent = parseFloat($("#percent-to-increase").val());
                item.DeliveryCost = window.ConvertPirce2(item.DeliveryCost + ((x - 1) * item.DeliveryCost * percent), "MYR", window.order_currency);
            });

            fillStatesControl(data);

            //_total = _total.minus(_delivery);
            //_delivery = new BigNumber(0);

            convertPrices(_country, country);
            refreshPrices(country);

            _country = country;
           // $("#parentDelivery").hide();

            if (cashOnDelivery) {
                $("#Country option[value=\"" + country + "\"]").prop("selected", true);
            }
            $("#Country").prop("disabled", cashOnDelivery);

            $("#hidden_country").val(_country);
        });
    };

    var initPaymentPage = function() {

        document.title = "Payment | Teeyoot";

        $(window).on("unload", function() {
            $("button").prop("disabled", false);
        });

        $(".payment-method:first")
            .css("text-decoration", "none")
            .css("color", "#ff4f00")
            .css("border-color", "#ff4f00");

        var defaultPaymentMethod = $(".payment-method:first").data("payment-method");
        var defaultPaymentNote = $(".payment-method:first").data("message");

        if (defaultPaymentMethod === "creditcard") {
            $("#paymentMethod").val("1");
        } else if (defaultPaymentMethod === "paypal") {
            $("#paymentMethod").val("2");
        } else if (defaultPaymentMethod === "mol") {
            $("#paymentMethod").val("3");
        } else if (defaultPaymentMethod === "cash") {
            $("#paymentMethod").val("4");
        } else if (defaultPaymentMethod == 'ipay-88') {
            $("#paymentMethod").val("5");
        } else if (defaultPaymentMethod == 'creditcard-bluesnap') {
            
            $("#paymentMethod").val("7");

        }
        else if(defaultPaymentMethod == '') {
            $("#paymentMethod").val("7");
        }

        $(".messages-payment-method").find(".message").text(defaultPaymentNote);

        $(".payment-method").click(function (event) {
            if(event.target.name!="payment"){
                $(this).find('input').prop("checked", true);
            }
            $(".payment-method")
                .css("text-decoration", "")
                .css("color", "")
                .css("border-color", "");

            $(this)
                .css("text-decoration", "none")
                .css("color", "#ff4f00")
                .css("border-color", "#ff4f00");

           // $(".payment-method-container").hide();

            var paymentMethod = $(this).data("payment-method");
            var note = $(this).data("message");
            //$("." + paymentMethod).show();
            var note;
            $(".cridit-card-info").hide('slow');
            if (paymentMethod == "creditcard") {
                $("#paymentMethod").val("1");
                //$(".contact-info").show('slow');
            } else if (paymentMethod == "paypal") {
                $("#paymentMethod").val("2");
                //$(".contact-info").show('slow');
            } else if (paymentMethod == "mol") {
                $("#paymentMethod").val("3");
                //$(".contact-info").show('slow');
            } else if (paymentMethod == "cash") {
                $("#paymentMethod").val("4");
                //$(".contact-info").show('slow');
            } else if (paymentMethod == "ipay-88") {
                $("#paymentMethod").val("5");
                //$(".contact-info").show('slow');
            } else if (paymentMethod == 'paypal_') {
                $("#paymentMethod").val("6");
                //$(".contact-info").show('slow');
            } else if (paymentMethod == 'creditcard-bluesnap') {
                x = $("#braintree-payment-form");

                $("#paymentMethod").val("7");
                //$(".contact-info").hide('slow');
                $(".cridit-card-info").show('slow');
                //return true;
            }


            var cashOnDelivery = false;

            var selectedPaymentMethod = $("#paymentMethod").val();
            if (selectedPaymentMethod === "4") {
                cashOnDelivery = true;
            }

            var country;

            if (cashOnDelivery) {
                country = $("#seller_country_id").val();
            } else {
                country = $("#Country option:selected").val();
            }

            changeCountryTo(country);

            $(".messages-payment-method").find(".message").text(note);
        });

        var selectedCountry = $("#Country option:selected").val();
        changeCountryTo(selectedCountry);

        $("#Country").change(function() {
            var country = $(this).val();
            changeCountryTo(country);
        });

        $("#State").change(function() {
            _total = _total.minus(_delivery);

            var delivery = $("#State option:selected").data("delivery-cost");
            setDelivery(delivery);

            var timeToDeliver = $("#State option:selected").data("time-to-deliver");

            $("#time-to-deliver").text(timeToDeliver);
            _total = _total.plus(_delivery);

            var state = $(this).val();
            if (state === "") {
                $("#parentDelivery").hide();
            } else {
                $("#parentDelivery").show();
            }

            refreshPrices(_country);
        });
    };

    return {
        init: function() {
            initPaymentPage();
            $("#paypal_").trigger('click');
        }
    };
}();