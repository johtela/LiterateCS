$(document).ready(function () {
    var sidebarMainMenu = $('#header-menu ul:first');
    var staticContent = $('#static-content');
    staticContent.find('h1').each(function () {
        sidebarMainMenu.append('<li id="' + $(this).attr('id') + '-menu"><a href="#' + $(this).attr('id') + '">' + $(this).html() + '</li>');
        title = sidebarMainMenu.find('#' + $(this).attr('id'));
    });
    for (var i = 2; i <= 3; i++) {
        AddHeaderLevel(i);
    }

    function AddHeaderLevel (level)
    {
        staticContent.find('h' + level).each(function () {
            prevTitle = sidebarMainMenu.find('#' + $(this)
                .prevAll('h' + (level - 1))
                .first()
                .attr('id').replace('.', '\\.') + '-menu');
            prevTitle.not(":has(ul)").append('<ul class="sub-menu"></ul>');
            prevTitle.find('.sub-menu').append('<li id="' + $(this)
                .attr('id') + '-menu"><a href="#' + $(this).attr('id') + '">' + $(this).html() + '</li>');
        });
    }

    // Sidebar toggling
    $("#sidebar-toggle").click(function(e) {
        e.preventDefault();
        if ($("#sidebar").is(":visible") && !$("#sidebar").hasClass("col-sm-4"))
            return;
        var sidebarVisible = "col-sm-4 col-xs-6 sidebar-popup";
        var sidebarHidden = "hidden-sm hidden-xs";
        var closeArrow = "fa-angle-double-left";
        var openArrow = "fa-angle-double-right";

		if ($("#sidebar").hasClass("col-sm-4")) {
            $("#sidebar").removeClass (sidebarVisible)
                .addClass(sidebarHidden);
            $("#sidebar-toggle-icon").removeClass(closeArrow)
                .addClass(openArrow);
		}
		else {
            $("#sidebar").removeClass (sidebarHidden)
                .addClass(sidebarVisible);
            $("#sidebar-toggle-icon").removeClass(openArrow)
                .addClass(closeArrow);
            window.scrollTo(0, 0);
		}
    });
});