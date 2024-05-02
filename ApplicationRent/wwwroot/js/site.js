const burger = document?.querySelector('[data-burger]');
const nav = document?.querySelector('[data-nav]');
const body = document.body;


burger?.addEventListener('click', () => {
    /*body.classList.toggle('stop-scroll');*/
    burger?.classList.toggle('burger-active');
    nav.classList.toggle('nav-visible');
});




// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
