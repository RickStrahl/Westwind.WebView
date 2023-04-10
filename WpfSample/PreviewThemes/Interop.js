import Vue from 'https://cdn.jsdelivr.net/npm/vue@2.6.12/dist/vue.esm.browser.js'

window.page = {
    person: {
        Firstname: "Rick",
        Lastname: "Strahl",
        Company: "West Wind",
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
        Object.assign(window.page.person, person);
    },
    // .NET calls this to explicitly retrieve the value
    getPerson() {
        return window.page.person;
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
    