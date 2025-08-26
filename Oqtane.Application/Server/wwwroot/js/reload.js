const scriptKeys = new Set();

export function onUpdate() {
    // determine if this is an initial request
    let initialRequest = scriptKeys.size === 0;

    // iterate over all script elements in document
    const scripts = document.getElementsByTagName('script');
    for (const script of Array.from(scripts)) {
        // only process scripts that include a data-reload attribute
        if (script.hasAttribute('data-reload')) {
            let key = getKey(script);

            if (!initialRequest) {
                // reload the script if data-reload is "always" or "true"... or if the script has not been loaded previously and data-reload is "once"
                let dataReload = script.getAttribute('data-reload');
                if ((dataReload === 'always' || dataReload === 'true') || (!scriptKeys.has(key) && dataReload == 'once')) {
                    reloadScript(script);
                }
            }

            // save the script key
            if (!scriptKeys.has(key)) {
                scriptKeys.add(key);
            }
        }
    }
}

function getKey(script) {
    if (script.src) {
        return script.src;
    } else if (script.id) {
        return script.id;
    } else {
        return script.innerHTML;
    }
}

function reloadScript(script) {
    try {
        if (isValid(script)) {
            injectScript(script);
        }
    } catch (error) {
        console.error(`Blazor Script Reload failed to load script: ${getKey(script)}`, error);
    }
}

function isValid(script) {
    if (script.innerHTML.includes('document.write(')) {
        console.log(`Blazor Script Reload does not support scripts using document.write(): ${script.innerHTML}`);
        return false;
    }
    return true;
}

function injectScript(script) {
    return new Promise((resolve, reject) => {
        var newScript = document.createElement('script');

        // replicate attributes and content
        for (let i = 0; i < script.attributes.length; i++) {
            if (script.attributes[i].name !== 'data-reload') {
                newScript.setAttribute(script.attributes[i].name, script.attributes[i].value);
            }
        }
        newScript.nonce = script.nonce; // must be referenced explicitly
        newScript.innerHTML = script.innerHTML;

        // dynamically injected scripts cannot be async or deferred
        newScript.async = false;
        newScript.defer = false;

        newScript.onload = () => resolve();
        newScript.onerror = (error) => reject(error);

        // inject script element in head to force execution in Blazor
        document.head.appendChild(newScript);

        // remove data-reload attribute
        script.removeAttribute('data-reload');
    });
}