﻿$(function() {
    $('#datetimepicker1').datetimepicker();
});

var vueObj;
vueObj = new Vue({
    el: '#app',
    data: {
        errors: [],
        name: "",
        date: "",
        agreement: "",
        amount: "",
        shares: "",
        isUSD: true,
        activeClass: 'active-price',
        notActiveClass: 'not-active-price'
    },
    methods: {
        add() {
            this.errors = [];
            this.date = $("#datepicker").val();
            if (this.name === "") this.errors.push("name");
            if (this.date === "") this.errors.push("date");
            if (this.agreement === "") this.errors.push("agreement");
            if (this.amount === "" || this.amount <= 0) this.errors.push("amount");
            if (this.shares === "" || this.shares <= 0) this.errors.push("shares");
            if (this.errors.length > 0) return;
            $("#add-button").attr("disabled", "disabled");
            $.post("/Panel/InsertInvestor",
                    {
                        Name: this.name,
                        Date: this.date,
                        Agreement: this.agreement,
                        AmountInvested: this.amount,
                        SharesReceived: this.shares
                    },
                function (data) {                        
                        if (data.statusCode === 200) {
                            setTimeout(function() { window.location.href = "/Panel/Index" }, 500);
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