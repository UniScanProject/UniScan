import {setFullscreen} from "./fullscreen.js";

/** @param {number} code
 * @param {string | undefined} reason */
export function onExit(code, reason) {
    /*
    RADIOACTIVE METHOD!!!!
    This place is a message... and part of a system of messages... pay attention to it!
    Sending this message was important to us. We considered ourselves to be a powerful culture.
    This place is not a place of honor... no highly esteemed deed is commemorated here... nothing valued is here.
    What is here was dangerous and repulsive to us. This message is a warning about danger.
    The danger is in a particular location... it increases towards a center... the center of danger is here... of a particular size and shape, and below us.
    The danger is still present, in your time, as it was in ours.
    The danger is to the body, and it can kill.
    The form of the danger is an emanation of energy.
    The danger is unleashed only if you substantially disturb this place physically. This place is best shunned and left uninhabited.
     */
    const d = globalThis.document || window.document;
    
    const splash = d.getElementById("avalonia-progress-notifier");
    const topbar = d.getElementById("topbar");

    setFullscreen(false, topbar, splash);
    
    if (code !== 0) {
        const html = `
            <md-dialog id="error-dialog" style="width: 35%">
                <div slot="headline">
                    UniScan has crashed!
                </div>
                <form slot="content" id="error-form" method="dialog">
                   <p>There may be more info in your browser's console.</p>
                   
                   ${((reason) => {
                       if (reason !== undefined) {
                           return `
                                <br>
                                <md-filled-text-field
                                    id="error-reason"
                                    type="textarea"
                                    label="Reason"
                                    rows="8"
                                    readOnly value="${reason}"
                                    style="width: 100%; resize: none; margin-bottom: 10px;">
                                </md-filled-text-field>
                                <md-filled-tonal-icon-button id="copy" type="button">
                                  <md-icon>content_copy</md-icon>
                                </md-filled-tonal-icon-button>
                            `;
                       }
                    })(reason)}
                </form>
                <div slot="actions">
                    <md-filled-button type="button" href="https://github.com/UniScanProject/UniScan/issues/new" target="_blank">Report bug</md-filled-button>
                    <md-filled-tonal-button form="error-form">Reload</md-filled-tonal-button>
                </div>
            </md-dialog>
        `;

        d.body.insertAdjacentHTML('afterbegin', html);
        
        const dialog = d.getElementById("error-dialog");
        dialog.addEventListener('closed', function () {
            window.location.reload();
        })
        
        const reasonTextbox = d.getElementById("error-reason");
        if (reasonTextbox !== undefined) {
            reasonTextbox.setSelectionRange(0, 0);
            
            const copy = d.getElementById("copy");
            
            copy.addEventListener('click', async function () {
               await navigator.clipboard.writeText(reasonTextbox.value);
            });
        }

        dialog.setAttribute('open', '');
    }
}