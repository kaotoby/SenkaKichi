$(function () {
    $(window).resize(function () {
        var $dropdown = $('.navbar-nav .dropdown-menu');
        $dropdown.css('max-height', ($(window).height() - $dropdown.offset().top) + 'px');
    });
});