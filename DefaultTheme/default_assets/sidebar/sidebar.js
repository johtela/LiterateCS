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
});