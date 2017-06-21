/* www.alsina.xyz | Alchemically bonded by Sebastian Alsina (@youngseebi) */

$(function () {

    // Smoothly Scroll to Links
    $('a[href*="#"]:not([href="#"])').click(function () {
        if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && location.hostname == this.hostname) {
            var target = $(this.hash);
            target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
            if (target.length) {
                $('html, body').animate({
                    scrollTop: target.offset().top - 50
                }, 300);
                return false;
            }
        }
    });
    
    var disablesite = false;
    if (disablesite == true) {
        document.title = 'GraphicBiz Imagine, LLC - Oops, this website is offline!';
        $("body").prepend("<style>.disconnected {width: 100%;height: 100%;padding: 0 22px;top: 0;bottom: 0;left: 0;right: 0;position: fixed;z-index: 9999;text-align: center;background-color: rgba(255, 255, 255, 0.95);color: #4c4c4c;font-family: 'Verdana', sans-serif;}.disconnected .box {max-width: 700px;margin: auto;}h1 {font-size: 3em;margin-bottom: 22px;line-height: 1.2em;}h2 {font-size: 1.6em;margin-bottom: 44px;}img {width: 100%;max-width: 300px;}.vertical-center {position: relative;top: 50%;-webkit-transform: translateY(-50%);-ms-transform: translateY(-50%);transform: translateY(-50%);}</style><div class=\"disconnected\"><div class=\"box vertical-center\"><h1>Site is disconnected from the Internet.</h1><h2>(We too are hoping it'll come back soon.)</h2><a href=\"http://graphicbiz.biz\"><img src=\"http://alsina.website/assets/Gz_Credit_2016.svg\" alt=\"\"></a></div></div>");
    }

});