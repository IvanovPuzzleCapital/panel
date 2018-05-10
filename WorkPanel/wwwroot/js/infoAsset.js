﻿$(function() {
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
            if (this.date === "") this.errors.push("date");            
            if (this.quantity === "" || this.quantity <= 0) this.errors.push("quantity");
            if (this.price === "" || this.price <= 0) this.errors.push("price");
            if (this.errors.length > 0) return;
            $("#add-button").attr("disabled", "disabled");
            $.post("/Portfolio/AddAsset",
                    {
                        Name: this.currency,
                        Date: this.date,
                        Quantity: this.quantity,
                        Price: this.price
                    },
                    function(data) {
                        if (data.statusCode === 200) {
                            setTimeout(function() { window.location.href = "/Portfolio/Index" }, 500);
                        }
                    })
                .fail(function(xhr, status, error) {
                    $("#add-button").removeAttr("disabled");
                });
        },

        togglePrice() {
            this.isUSD = !this.isUSD;
        }
    }
});