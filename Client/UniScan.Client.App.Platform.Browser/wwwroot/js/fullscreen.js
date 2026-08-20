/**
 * @param {boolean} fullscreen
 * @param {HTMLElement} topbar
 * @paarm {HTMLElement} splash
 */
export function setFullscreen(fullscreen, topbar, splash) {
    if (fullscreen === true) {
        topbar.classList.add("collapse");
        splash.classList.add("fullscreen");
    } else {
        topbar.classList.remove("collapse");
        splash.classList.remove("fullscreen");
    }
}