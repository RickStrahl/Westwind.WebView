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
        emojiUpdated: function(iconText){
            page.dotnet.getDotnet().EmojiUpdated(iconText ?? '');
        }
    },
    initialize: function(mode) {
        $(".emoji-container").on("click", "a", function() {            
            var $el = $(this);
            var iconText = this.title;

            if (iconText){                
                if (page.dotnet.isDotnet()) {
                    page.dotnet.emojiUpdated(iconText ?? '');
                }
                else
                    alert('selected ' + iconText);
            }
        });
        $("#Search").focus();

        var keyupFn = debounce(page.searchEmoji, 150);
        $("#Search")
            .keyup( keyupFn )
            .keydown(function (e) {
                var key = e.keyCode;                
                if (key == 8 || key == 46) // backspace
                   keyupFn();                            
            });


        

    },
    searchEmoji: function searchEmoji(search) {        
        if (typeof search !== "string")
            search = this.value;

        $(".emoji-container>a").show();        
        if (!search) {
            return; 
        }

        $(".emoji-container>a").each(function() {
            var title = this.title;       
            
            if (title.toLowerCase().indexOf(search.toLowerCase()) > -1)
              return;

            this.style.display = "none";
        });            
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
