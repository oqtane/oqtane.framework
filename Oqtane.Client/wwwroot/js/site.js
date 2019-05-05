/* called by index.html to check if mode is set */
function getCookie(name) {
    name = name + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) === ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) === 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

/* Open when someone clicks on the span element */
function openActions() {
    document.getElementById("actions").style.width = "25%";
}

/* Close when someone clicks on the "x" symbol inside the overlay */
function closeActions() {
    document.getElementById("actions").style.width = "0%";
}

