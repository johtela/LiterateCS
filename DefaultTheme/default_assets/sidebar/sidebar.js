$(document).ready(function () {
    var navbarHeight = $(".navbar").height();
    $("body").css({ "padding-top": navbarHeight + "px" });

    var sidebarMainMenu = $('#header-menu ul:first');
    var staticContent = $('#static-content');
    staticContent.find('h1').each(function () {
        sidebarMainMenu.append('<li id="' + $(this).attr('id') + '-menu"><a class="page-link" href="#' + $(this).attr('id') + '">' + $(this).html() + '</li>');
        title = sidebarMainMenu.find('#' + $(this).attr('id'));
    });
    for (var i = 2; i <= 3; i++) {
        addHeaderLevel(i);
    }

    function addHeaderLevel (level)
    {
        staticContent.find('h' + level).each(function () {
            prevTitle = sidebarMainMenu.find('#' + $(this)
                .prevAll('h' + (level - 1))
                .first()
                .attr('id').replace('.', '\\.') + '-menu');
            prevTitle.not(":has(ul)").append('<ul class="sub-menu"></ul>');
            prevTitle.find('.sub-menu').append('<li id="' + $(this)
                .attr('id') + '-menu"><a class="page-link" href="#' + $(this).attr('id') + '">' + $(this).html() + '</li>');
        });
    }

    // Sidebar toggling
    var sidebarVisible = "col-sm-4 col-xs-6 sidebar-popup";
    var sidebarHidden = "hidden-sm hidden-xs";
    var closeArrow = "fa-angle-double-left";
    var openArrow = "fa-angle-double-right";

    function isSidebarOpen() {
        return $("#sidebar").hasClass("col-sm-4");
    }

    function closeSidebar() {
        $("#sidebar").removeClass(sidebarVisible)
            .addClass(sidebarHidden);
        $("#sidebar-toggle-icon").removeClass(closeArrow)
            .addClass(openArrow);
    }

    function openSidebar() {
        $("#sidebar").removeClass(sidebarHidden)
            .addClass(sidebarVisible);
        $("#sidebar-toggle-icon").removeClass(openArrow)
            .addClass(closeArrow);
    }

    $("#sidebar-toggle").click(function(e) {
        e.preventDefault();
        if ($("#sidebar").is(":visible") && !isSidebarOpen()) {
            window.scrollTo(0, 0);
        }
        else if (isSidebarOpen()) {
            closeSidebar();
		}
        else {
            openSidebar();
            window.scrollTo(0, 0);
		}
    });

    $(".page-link").click(function (e) {
        $(window).bind('hashchange', function (e) {
            window.scrollBy(0, -navbarHeight - 8);
            $(window).unbind('hashchange');
        });
        if (isSidebarOpen ())
            closeSidebar();
    });
});