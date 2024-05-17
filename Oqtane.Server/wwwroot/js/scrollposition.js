
function setScrollPosition() {
    window.scrollTo({
        top: 0,
        left: 0,
        behavior: 'instant'
    });
}

export function onUpdate() {
    setScrollPosition();
}