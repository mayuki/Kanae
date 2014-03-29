// ==UserScript==
// @name        Inline image preview for ChatWork+Kanae ({{domain}})
// @namespace   github.com/mayuki/Kanae
// @description Inline image preview for ChatWork+Kanae ({{domain}})
// @include     https://www.chatwork.com/*
// @include     https://kcw.kddi.ne.jp/*
// @version     1.1
// ==/UserScript==
(function(){
    "use strict";
    
    var targetDomain = "{{domain}}";
    var styleE = document.createElement('style');
    styleE.innerHTML = '.chatTimeLineMessageArea a[href^="http://' + targetDomain + '/-/"], .chatTimeLineMessageArea a[href^="https://' + targetDomain + '/-/"] { position: relative; } ' +
                       '.chatTimeLineMessageArea a[href^="http://' + targetDomain + '/-/"]::after, .chatTimeLineMessageArea a[href^="https://' + targetDomain + '/-/"]::after { display: block; height: 170px; content: ""; }';
    document.head.appendChild(styleE);
    
    var lockMutationEvent = false;
    var observer = new WebKitMutationObserver(function (mutations) {
        if (lockMutationEvent) return;
        lockMutationEvent = true;
        mutations.forEach(function (mutation) {
            for (var i = 0; i < mutation.addedNodes.length; i++) {
                if (mutation.addedNodes[i].classList && mutation.addedNodes[i].classList.contains('chatTimeLineMessage')) {
                    var node = mutation.addedNodes[i];
                    var anchors = node.querySelectorAll('a[href^="http://' + targetDomain + '/-/"], a[href^="https://' + targetDomain + '/-/"]');
                    [].forEach.call(anchors, function (elem) {
                        var imgE = document.createElement('img');
                        imgE.src = elem.href;
                        imgE.alt = '';
                        imgE.height = 160;
                        imgE.style.display = 'block';
                        imgE.style.position = 'absolute';
                        imgE.style.bottom = '0';
                        elem.appendChild(imgE);
                    });
                }
            }
        });
        lockMutationEvent = false;
    });
    observer.observe(document.getElementById('_chatContent'), { childList:true, subtree: true });
})();