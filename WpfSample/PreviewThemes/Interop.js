import Vue from 'https://cdn.jsdelivr.net/npm/vue@2.6.12/dist/vue.esm.browser.js'

window.page = {
    person: {
        Firstname: "Rick",
        Lastname: "Strahl",
        Company: "Wst Wind",
        Email: "",
        Address: {
            Street: "",
            City: "Paia, Hawaii",
            State: "",
            Zip: "",
            Country: ""
        }
    },
    updatePerson(person) {

    }
}

// *** Initialize Vue for the Page *** //
var vueApp;

window.Initialize = function() {
    vueApp = new Vue({
        el: '#app',
        data: function () {        
            var model =  { Person: window.page.person };
            return model;
        },
    });
}   

window.Initialize();
    