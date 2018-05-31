$(function () {
    $('#datetimepicker1').datetimepicker();
});

var vueObj;
vueObj = new Vue({
    el: '#app',
    data: {
        errors: [],
        currency: "",
        date: "",
        quantity: "",
        price: "",
        isUSD: true,
        activeClass: 'active-price',
        notActiveClass: 'not-active-price'
    },
    methods: {
        add() {
            this.errors = [];
            this.date = $("#datetimepicker").val();
            if (this.currency === "") this.errors.push("currency");
            if (this.currency === "Bitcoin" && !this.isUSD) this.errors.push("wrong-currency");
            if (this.date === "") this.errors.push("date");
            if (this.quantity === "" || this.quantity <= 0) this.errors.push("quantity");
            if (this.price === "" || this.price <= 0) this.errors.push("price");
            if (this.errors.length > 0) return;
            $("#add-button").attr("disabled", "disabled");
            axios({
                    method: 'post',
                    url: '/Portfolio/AddAsset',
                    data: {
                        'Name': this.currency,
                        'Date': this.date,
                        'Quantity': this.quantity,
                        'Price': this.price,
                        'PurchaseType': this.isUSD ? "USD" : "BTC"
                    }
                })
                .then(function(response) {
                    var data = response.data;
                    if (data.statusCode === 200) {
                        switch (data.errorCode) {
                        case 3:
                            vueObj.errors.push("not-found-on-server");
                            $("#add-button").removeAttr("disabled");
                            break;
                        case 2:
                            vueObj.errors.push("not-enough");
                            $("#add-button").removeAttr("disabled");
                            break;
                        case 1:
                            vueObj.errors.push("not-found");
                            $("#add-button").removeAttr("disabled");
                            break;
                        default:
                            setTimeout(function() { window.location.href = "/Portfolio/Index" }, 500);
                            break;
                        }
                    }
                })
                .catch(function(error) {
                    $("#add-button").removeAttr("disabled");
                });
        },
        togglePrice() {
            this.isUSD = !this.isUSD;
        }
    }
});