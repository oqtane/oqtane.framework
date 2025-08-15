function subMenu(a) {
    event.preventDefault();
    event.stopPropagation();

    var li = a.parentElement, submenu = li.getElementsByTagName('ul')[0];
    submenu.style.display = submenu.style.display == "block" ? "none" : "block";
    return false;
}