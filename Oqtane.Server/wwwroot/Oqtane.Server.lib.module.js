const pageScriptInfoBySrc = new Map();

function getKey(element) {
    if (element.src !== "") {
        return element.src;
    } else {
        return element.content;
    }
}

function registerPageScript(element) {
    let key = getKey(element);
    let pageScriptInfo = pageScriptInfoBySrc.get(key);

    if (pageScriptInfo) {
        pageScriptInfo.referenceCount++;
    } else {
        if (element.src.startsWith("./")) {
            element.src = new URL(element.src.substr(2), document.baseURI).toString();
        }
        pageScriptInfo = { src: element.src, type: element.type, integrity: element.integrity, crossorigin: element.crossorigin, content: element.content, location: element.location, reload: element.reload, module: null, referenceCount: 1 };
        pageScriptInfoBySrc.set(key, pageScriptInfo);
        initializePageScript(key, pageScriptInfo);
    }
}

async function initializePageScript(key, pageScriptInfo) {
    if (pageScriptInfo.reload) {
        const module = await import(pageScriptInfo.src);
        pageScriptInfo.module = module;
        module.onLoad?.();
        module.onUpdate?.();
    } else {
        if (!scriptExists(pageScriptInfo)) {
            try {
                injectScript(pageScriptInfo);
            } catch (error) {
                if (pageScriptInfo.src !== "") {
                    console.error("Failed to load external script: ${pageScriptInfo.src}", error);
                } else {
                    console.error("Failed to load inline script: ${pageScriptInfo.content}", error);
                }
            }
        }
    }
    removePageScript(key, pageScriptInfo);
}

function onEnhancedLoad() {
    for (const [key, pageScriptInfo] of pageScriptInfoBySrc) {
        if (pageScriptInfo.referenceCount <= 0) {
            if (pageScriptInfo.module) {
                pageScriptInfo.module.onDispose?.();
            }
            pageScriptInfoBySrc.delete(key);
        }
    }

    for (const [key, pageScriptInfo] of pageScriptInfoBySrc) {
        if (pageScriptInfo.module) {
            pageScriptInfo.module.onUpdate?.();
        } else {
            if (!scriptExists(pageScriptInfo)) {
                try {
                    injectScript(pageScriptInfo);
                } catch (error) {
                    if (pageScriptInfo.src !== "") {
                        console.error("Failed to load external script: ${pageScriptInfo.src}", error);
                    } else {
                        console.error("Failed to load inline script: ${pageScriptInfo.content}", error);
                    }
                }
            }
        }
    }

    for (const [key, pageScriptInfo] of pageScriptInfoBySrc) {
        removePageScript(key, pageScriptInfo);
    }
}

function scriptExists(pageScriptInfo) {
    if (pageScriptInfo.src !== "") {
        return document.querySelector("script[src=\"" + pageScriptInfo.src + "\"]");
    } else {
        const scripts = document.querySelectorAll('script:not([src])');
        for (let i = 0; i < scripts.length; i++) {
            if (scripts[i].textContent.includes(pageScriptInfo.content)) {
                return true;
            }
        }
        return false;
    }
}

function injectScript(pageScriptInfo) {
    return new Promise((resolve, reject) => {
        var script = document.createElement("script");

        script.async = false;
        if (pageScriptInfo.type !== "") {
            script.type = pageScriptInfo.type;
        }

        if (pageScriptInfo.src !== "") {
            script.src = pageScriptInfo.src;
            if (pageScriptInfo.integrity !== "") {
                script.integrity = pageScriptInfo.integrity;
            }
            if (pageScriptInfo.crossorigin !== "") {
                script.crossOrigin = pageScriptInfo.crossorigin;
            }
        } else {
            script.innerHTML = pageScriptInfo.content;
        }

        script.onload = () => resolve();
        script.onerror = (error) => reject(error);

        // add script to page
        document.head.appendChild(script);
    });
}

function removePageScript(key, pageScriptInfo) {
    var pageScript;

    if (pageScriptInfo.src !== "") {
        pageScript = document.querySelector("page-script[src=\"" + key + "\"]");
    } else {
        pageScript = document.querySelector("page-script[content=\"" + CSS.escape(pageScriptInfo.content) + "\"]");
    }

    if (pageScript) {
        pageScript.remove();
    }
}

function unregisterPageScript(element) {
    const pageScriptInfo = pageScriptInfoBySrc.get(getKey(element));
    if (!pageScriptInfo) {
        return;
    }
    pageScriptInfo.referenceCount--;
}

export function afterWebStarted(blazor) {
    customElements.define('page-script', class extends HTMLElement {
        static observedAttributes = ['src', 'type', 'integrity', 'crossorigin', 'content', 'reload'];

        constructor() {
            super();

            this.src = "";
            this.type = "";
            this.integrity = "";
            this.crossorigin = "";
            this.content = "";
            this.reload = false;
        }

        attributeChangedCallback(name, oldValue, newValue) {
            switch (name) {
                case "src":
                    this.src = newValue;
                    break;
                case "type":
                    this.type = newValue;
                    break;
                case "integrity":
                    this.integrity = newValue;
                    break;
                case "crossorigin":
                    this.crossorigin = newValue;
                    break;
                case "content":
                    this.content = newValue;
                    break;
                case "reload":
                    this.reload = newValue;
                    break;
            }

            // if last attribute for element has been processed
            if (this.attributes[this.attributes.length - 1].name === name) {
                registerPageScript(this);
            }
        }

        disconnectedCallback() {
            unregisterPageScript(this);
        }
    });

    blazor.addEventListener('enhancedload', onEnhancedLoad);
}