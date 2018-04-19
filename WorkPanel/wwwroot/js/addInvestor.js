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
    },
    methods: {
        add() {
            this.errors = [];
            if (this.name === "") this.errors.push("name");
            if (this.date === "") this.errors.push("date");
            if (this.agreement === "") this.errors.push("agreement");
            if (this.amount === "") this.errors.push("amount");
            if (this.shares === "") this.errors.push("shares");
            if (this.errors.length > 0) return;
            $.post("/Panel/InsertInvestor",
                    {
                        Name: this.name,
                        Date: this.date,
                        Agreement: this.agreement,
                        AmountInvested: this.amount,
                        SharesReceived: this.shares
                    },
                function (data) {
                        this.disableButtons = false;
                        if (data.statusCode === 200) {
                            setTimeout(function() { window.location.href = "/Panel/Index" }, 500);
                        }
                    })
                .fail(function(xhr, status, error) {
                    //TODO обработать ошибку
                });
        }
    }
});