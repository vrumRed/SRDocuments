var isClosed;

function detectmob() {
    if (window.innerWidth <= 900 && window.innerHeight <= 700) {
        return true;
    } else {
        return false;
    }
}

$(document).ready(function () {
    isClosed = !detectmob();
    openNav();
});

function openNav() {
    if (isClosed) {
        document.getElementById("mySidenav").style.width = "250px";
        document.getElementById("nav-container").style.paddingLeft = "270px";
        isClosed = false;
    }
    else {
        document.getElementById("mySidenav").style.width = "0px";
        document.getElementById("nav-container").style.paddingLeft = "25px";
        isClosed = true;
    }
    
}

//$(document).ready(function () {
//    var trigger = $('.hamburger'),
//        isClosed = false;

//    var triggerRender = $("#renderContent");
//    var triggerButton = $("#renderButton");

//    trigger.click(function () {
//        hamburger_cross();
//    });

//    function hamburger_cross() {

//        if (isClosed == true) {
//            trigger.removeClass('is-closed');
//            trigger.addClass('is-open');
//            triggerRender.removeClass('col-xs-12');
//            triggerRender.addClass('col-xs-offset-3');
//            triggerRender.addClass('col-xs-9');
//            triggerButton.addClass('col-xs-offset-3');
//            triggerRender.removeClass('col-lg-12');
//            triggerRender.addClass('col-lg-offset-2');
//            triggerRender.addClass('col-lg-10');
//            triggerButton.addClass('col-lg-offset-2');
//            isClosed = false;
//        } else {
//            trigger.removeClass('is-open');
//            trigger.addClass('is-closed');
//            triggerRender.addClass('col-xs-12');
//            triggerRender.removeClass('col-xs-offset-3');
//            triggerRender.removeClass('col-xs-9');
//            triggerButton.removeClass('col-xs-offset-3');
//            triggerRender.addClass('col-lg-12');
//            triggerRender.removeClass('col-lg-offset-2');
//            triggerRender.removeClass('col-lg-10');
//            triggerButton.removeClass('col-lg-offset-2');

//            isClosed = true;
//        }
//    }

//    $('[data-toggle="offcanvas"]').click(function () {
//        $('#wrapper').toggleClass('toggled');
//    });
//});