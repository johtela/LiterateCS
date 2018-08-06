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
        var sidebarVisible = "col-sm-4 col-xs-5";
        var sidebarHidden = "hidden-sm hidden-xs";
        var contentWithSidebar = "col-sm-8 col-xs-7";
		if ($("#sidebar").hasClass("col-sm-4")) {
            $("#contentarea").removeClass(contentWithSidebar);
            $("#sidebar").removeClass (sidebarVisible)
				.addClass (sidebarHidden);
			$("#sidebar-toggle-icon").removeClass ("fa-angle-double-left")
				.addClass ("fa-book");
		}
		else {
            $("#contentarea").addClass(contentWithSidebar);
            $("#sidebar").removeClass (sidebarHidden)
				.addClass (sidebarVisible);
			$("#sidebar-toggle-icon").removeClass ("fa-book")
				.addClass ("fa-angle-double-left");
		}
    });
});