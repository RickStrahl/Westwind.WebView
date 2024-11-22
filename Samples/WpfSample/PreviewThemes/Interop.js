import Vue from 'https://cdn.jsdelivr.net/npm/vue@2.6.12/dist/vue.esm.browser.js'

window.page = {
    person: {
        firstname: "Rick",
        lastname: "Strahl",
        company: "West Wind",
        email: "",
        address: {
            street: "111 Blue Skies Blvd.",
            city: "Paia, Hawaii",
            state: "",
            zip: "",
            country: ""
        }        
    },
    updatePerson(person) {        
        Object.assign(window.page.person, person);
        console.log(person, window.page.person);               
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
            var model = { person: window.page.person };            
            return model;
        },
    });
}   

window.Initialize();
    