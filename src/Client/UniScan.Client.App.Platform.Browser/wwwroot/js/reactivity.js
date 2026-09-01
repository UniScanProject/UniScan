import {setFullscreen} from "./fullscreen.js";

//copied from https://developer.mozilla.org/en-US/docs/Web/API/MutationObserver#example
const targetNode = document.getElementById("avalonia-progress-notifier");
const topbar = document.getElementById("topbar");

let cur = false;

// Create an observer instance linked to the callback function
const observer = new MutationObserver(() => {
    const close = targetNode.classList.contains("splash-close");
    
    if (cur !== close) {
        cur = close;
        setFullscreen(close, topbar, targetNode);
    }
});

// Start observing the target node for configured mutations
observer.observe(targetNode, { attributes: true, attributeFilter: ["class"], subtree: false });