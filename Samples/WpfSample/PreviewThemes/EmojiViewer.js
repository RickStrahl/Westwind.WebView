debugger;
var page = {

    dotnet: {
        getDotnet: function() {
            return window.chrome.webview.hostObjects.sync.dotnet;
        },
        getDotnetAsync: function() {
            return window.chrome.webview.hostObjects.dotnet;
        },
        isDotnet: function() {
            return window.chrome.webview.hostObjects ? true : false;
        },
        emojiUpdated: async function(iconText){
            page.dotnet.getDotnet().EmojiUpdated(iconText ?? '');
        }
    },
    initialize: function(mode) {
        $(".emoji-container").on("click", "a", function (e) {
            const iconText = this.title ?? '';

            if (iconText){                
                if (page.dotnet.isDotnet()) {
                    page.dotnet.emojiUpdated(iconText ?? '');
                }
                else
                    alert('selected ' + iconText);
            }
            e.preventDefault();
        });
        $(document.body).on("contextmenu", function (e) {           
            e.preventDefault();
        })
        .on("contextmenu", "a", function (e) {
            const iconText = this.title ?? '';
 
            if (iconText) {
                if (page.dotnet.isDotnet()) {
                    page.dotnet.emojiUpdated(iconText , true);  // clipobard
                }
            }

            e.preventDefault();
        });


        

    },
    searchEmoji: function searchEmoji(search) {        
        if (typeof search !== "string")
            search = this.value;

        var $items = $(".emoji-container>a");
        var total = $items.length;

        // show all
        $items.show();
        if (!search) {
            return total; 
        }

        var count = 0;
        for (var i = 0; i < total; i++) {
            var a$ = $items[i];
            var title = a$.title;

            if (title.toLowerCase().indexOf(search.toLowerCase()) > -1) {
                count++;
                continue;
            }
                        
            a$.style.display = "none";
        }

        return count;           
    }
};  // page


$.expr[":"].containsNoCase = function(el, i, m) {
    var search = m[3];
    if (!search) return false;
    return new RegExp(search, "i").test($(el).text());
};
page.initialize();

function initializeInterop() {
  
}


function debounce(func, wait, immediate) {
    var timeout;
    return function () {
      var context = this, args = arguments;
      var later = function () {
        timeout = null;
        if (!immediate) func.apply(context, args);
      };
      var callNow = immediate && !timeout;
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
      if (callNow)
        func.apply(context, args);
    };
  };
