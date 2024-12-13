const pageScriptInfoBySrc = new Map();

function getKey(element) {
    if (element.src !== "") {
        return element.src;
    } else {
        return element.content;
    }
}

function registerPageScriptElement(element) {
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
        initializePageScript(pageScriptInfo);
    }
}

function unregisterPageScriptElement(element) {
    const pageScriptInfo = pageScriptInfoBySrc.get(getKey(element));
    if (!pageScriptInfo) {
        return;
    }
    pageScriptInfo.referenceCount--;
}

async function initializePageScript(pageScriptInfo) {
    if (pageScriptInfo.reload) {
        const module = await import(pageScriptInfo.src);
        pageScriptInfo.module = module;
        module.onLoad?.();
        module.onUpdate?.();
    } else {
        try {
            injectScript(pageScriptInfo);
        } catch (error) {
            console.error("Failed to load script: ${pageScriptInfo.src}", error);
        }
    }
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
            try {
                injectScript(pageScriptInfo);
            } catch (error) {
                if (pageScriptInfo.src !== "") {
                    console.error("Failed to load script library: ${pageScriptInfo.src}", error);
                } else {
                    console.error("Failed to load inline script: ${pageScriptInfo.content}", error);
                }
            }
        }
    }
}

function injectScript(pageScriptInfo) {
    return new Promise((resolve, reject) => {
        var pageScript;
        var script = document.createElement("script");
        script.async = false;

        if (pageScriptInfo.src !== "") {
            pageScript = document.querySelector("page-script[src=\"" + pageScriptInfo.src + "\"]");
            script.src = pageScriptInfo.src;
            if (pageScriptInfo.type !== "") {
                script.type = pageScriptInfo.type;
            }
            if (pageScriptInfo.integrity !== "") {
                script.integrity = pageScriptInfo.integrity;
            }
            if (pageScriptInfo.crossorigin !== "") {
                script.crossOrigin = pageScriptInfo.crossorigin;
            }
        } else {
            pageScript = document.querySelector("page-script[content=\"" + CSS.escape(pageScriptInfo.content) + "\"]");
            script.innerHTML = pageScriptInfo.content;
        }

        script.onload = () => resolve();
        script.onerror = (error) => reject(error);

        // add script to page
        if (pageScriptInfo.location === "head") {
            document.head.appendChild(script);
        } else {
            document.body.appendChild(script);
            // note this throws an exception when page-script is on a page which has interactive components (ie Counter.razor)
            // Error: Uncaught (in promise) TypeError: can't access property "attributes" of null
            // this seems to be related to Blazor-Server-Component-State which is also injected at the end of the body
        }

        // remove page-script element from page
        if (pageScript !== null) {
            pageScript.remove();
        }
    });
}

export function afterWebStarted(blazor) {
    customElements.define('page-script', class extends HTMLElement {
        static observedAttributes = ['src', 'type', 'integrity', 'crossorigin', 'content', 'location', 'reload'];

        constructor() {
            super();

            this.src = "";
            this.type = "";
            this.integrity = "";
            this.crossorigin = "";
            this.content = "";
            this.location = "head";
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
                case "location":
                    this.location = newValue;
                    break;
                case "reload":
                    this.reload = newValue;
                    break;
            }
        }

        connectedCallback() {
            registerPageScriptElement(this);
        }

        disconnectedCallback() {
            unregisterPageScriptElement(this);
        }
    });

    blazor.addEventListener('enhancedload', onEnhancedLoad);
}