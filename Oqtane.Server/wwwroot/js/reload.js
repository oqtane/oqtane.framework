const scriptInfoBySrc = new Map();

function getKey(script) {
    if (script.hasAttribute("src") && script.src !== "") {
        return script.src;
    } else {
        return script.innerHTML;
    }
}

export function onUpdate() {
    let timestamp = Date.now();
    let enhancedNavigation = scriptInfoBySrc.size !== 0;

    // iterate over all script elements in page
    const scripts = document.getElementsByTagName("script");
    for (const script of Array.from(scripts)) {
        let key = getKey(script);
        let scriptInfo = scriptInfoBySrc.get(key);
        if (!scriptInfo) {
            // new script added
            scriptInfo = { timestamp: timestamp };
            scriptInfoBySrc.set(key, scriptInfo);
            if (enhancedNavigation) {
                reloadScript(script);
            }
        } else {
            // existing script
            scriptInfo.timestamp = timestamp;
            if (script.hasAttribute("data-reload") && script.getAttribute("data-reload") === "true") {
                reloadScript(script);
            }
        }
    }

    // remove scripts that are no longer referenced
    for (const [key, scriptInfo] of scriptInfoBySrc) {
        if (scriptInfo.timestamp !== timestamp) {
            scriptInfoBySrc.delete(key);
        }
    }
}

function reloadScript(script) {
    try {
        replaceScript(script);
    } catch (error) {
        if (script.hasAttribute("src") && script.src !== "") {
            console.error("Failed to load external script: ${script.src}", error);
        } else {
            console.error("Failed to load inline script: ${script.innerHtml}", error);
        }
    }
}

function replaceScript(script) {
    return new Promise((resolve, reject) => {
        var newScript = document.createElement("script");

        // replicate attributes and content
        for (let i = 0; i < script.attributes.length; i++) {
            newScript.setAttribute(script.attributes[i].name, script.attributes[i].value);
        }
        newScript.innerHTML = script.innerHTML;

        // dynamically injected scripts cannot be async or deferred
        newScript.async = false;
        newScript.defer = false;

        newScript.onload = () => resolve();
        newScript.onerror = (error) => reject(error);

        // remove existing script
        script.remove();

        // replace with new script to force reload in Blazor
        document.head.appendChild(newScript);
    });
}

