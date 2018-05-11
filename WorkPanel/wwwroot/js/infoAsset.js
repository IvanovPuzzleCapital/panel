$(function() {
    $('#datetimepicker-info').datetimepicker();
});

 var vue = new Vue({
     el: '#app-info',
     data: {
         sellQuantity: "",
         sellPrice: "",
         isUSD: true,
         activeClass: 'active-price',
         notActiveClass: 'not-active-price'
     },
     methods: {
         add() {

         },

         togglePrice() {
             this.isUSD = !this.isUSD;
         }

     }
 });