var Oqtane = Oqtane || {};

Oqtane.Interop = {
    setCookie: function (name, value, days, secure, sameSite) {
        var d = new Date();
        d.setTime(d.getTime() + (days * 24 * 60 * 60 * 1000));
        var expires = "expires=" + d.toUTCString();
        var cookieString = name + "=" + value + ";" + expires + ";path=/";
        if (secure) {
            cookieString += "; secure";
        }
        if (sameSite === "Lax" || sameSite === "Strict" || sameSite === "None") {
            cookieString += "; SameSite=" + sameSite;
        }
        document.cookie = cookieString;
    },
    getCookie: function (name) {
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
    },
    updateTitle: function (title) {
        if (document.title !== title) {
            document.title = title;
        }
    },
    includeMeta: function (id, attribute, name, content) {
        var meta = document.querySelector("meta[" + attribute + "=\"" + CSS.escape(name) + "\"]");
        if (meta === null) {
            meta = document.createElement("meta");
            meta.setAttribute(attribute, name);
            if (id !== "") {
                meta.id = id;
            }
            meta.content = content;
            document.head.appendChild(meta);
        }
        else {
            if (id !== "") {
                meta.setAttribute("id", id);
            }
            if (meta.content !== content) {
                meta.setAttribute("content", content);
            }
        }
    },
    includeLink: function (id, rel, href, type, integrity, crossorigin, insertbefore) {
        var link = document.querySelector("link[href=\"" + CSS.escape(href) + "\"]");
        if (link === null) {
            link = document.createElement("link");
            if (id !== "") {
                link.id = id;
            }
            link.rel = rel;
            if (type !== "") {
                link.type = type;
            }
            link.href = href;
            if (integrity !== "") {
                link.integrity = integrity;
            }
            if (crossorigin !== "") {
                link.crossOrigin = crossorigin;
            }
            if (insertbefore === "") {
                document.head.appendChild(link);
            }
            else {
                var sibling = document.getElementById(insertbefore);
                sibling.parentNode.insertBefore(link, sibling);
            }
        }
        else {
            if (link.id !== id) {
                link.setAttribute('id', id);
            }
            if (link.rel !== rel) {
                link.setAttribute('rel', rel);
            }
            if (type !== "") {
                if (link.type !== type) {
                    link.setAttribute('type', type);
                }
            } else {
                link.removeAttribute('type');
            }
            if (link.href !== this.getAbsoluteUrl(href)) {
                link.removeAttribute('integrity');
                link.removeAttribute('crossorigin');
                link.setAttribute('href', href);
            }
            if (integrity !== "") {
                if (link.integrity !== integrity) {
                    link.setAttribute('integrity', integrity);
                }
            } else {
                link.removeAttribute('integrity');
            }
            if (crossorigin !== "") {
                if (link.crossOrigin !== crossorigin) {
                    link.setAttribute('crossorigin', crossorigin);
                }
            } else {
                link.removeAttribute('crossorigin');
            }
        }
    },
    includeLinks: function (links) {
        for (let i = 0; i < links.length; i++) {
            this.includeLink(links[i].id, links[i].rel, links[i].href, links[i].type, links[i].integrity, links[i].crossorigin, links[i].insertbefore);
        }
    },
    includeScript: function (id, src, integrity, crossorigin, type, content, location, dataAttributes) {
        var script;
        if (src !== "") {
            script = document.querySelector("script[src=\"" + CSS.escape(src) + "\"]");
        }
        else {
            if (id !== "") {
                script = document.getElementById(id);
            } else {
                const scripts = document.querySelectorAll("script:not([src])");
                for (let i = 0; i < scripts.length; i++) {
                    if (scripts[i].textContent.includes(content)) {
                        script = scripts[i];
                    }
                }
            }
        }
        if (script !== null) {
            script.remove();
            script = null;
        }
        if (script === null) {
            script = document.createElement("script");
            if (id !== "") {
                script.id = id;
            }
            if (type !== "") {
                script.type = type;
            }
            if (src !== "") {
                script.src = src;
                if (integrity !== "") {
                    script.integrity = integrity;
                }
                if (crossorigin !== "") {
                    script.crossOrigin = crossorigin;
                }
            }
            else {
                script.innerHTML = content;
            }
            if (dataAttributes !== null) {
                for (var key in dataAttributes) {
                    script.setAttribute(key, dataAttributes[key]);
                }
            }

            try {
                this.addScript(script, location);
            } catch (error) {
                if (src !== "") {
                    console.error("Failed to load external script: ${src}", error);
                } else {
                    console.error("Failed to load inline script: ${content}", error);
                }
            }
        }
    },
    addScript: function (script, location) {
        return new Promise((resolve, reject) => {
            script.async = false;
            script.defer = false;

            script.onload = () => resolve();
            script.onerror = (error) => reject(error);

            if (location === 'head') {
                document.head.appendChild(script);
            } else {
                document.body.appendChild(script);
            }
        });
    },
    includeScripts: async function (scripts) {
        const bundles = [];
        for (let s = 0; s < scripts.length; s++) {
            if (scripts[s].bundle === '') {
                scripts[s].bundle = scripts[s].href;
            }
            if (!bundles.includes(scripts[s].bundle)) {
                bundles.push(scripts[s].bundle);
            }
        }
        const promises = [];
        for (let b = 0; b < bundles.length; b++) {
            const urls = [];
            for (let s = 0; s < scripts.length; s++) {
                if (scripts[s].bundle === bundles[b]) {
                    urls.push(scripts[s].href);
                }
            }
            promises.push(new Promise((resolve, reject) => {
                if (loadjs.isDefined(bundles[b])) {
                    loadjs.ready(bundles[b], () => {
                        resolve(true);
                    });
                }
                else {
                    loadjs(urls, bundles[b], {
                        async: false,
                        returnPromise: true,
                        before: function (path, element) {
                            for (let s = 0; s < scripts.length; s++) {
                                if (path === scripts[s].href) {
                                    if (scripts[s].integrity !== '') {
                                        element.integrity = scripts[s].integrity;
                                    }
                                    if (scripts[s].crossorigin !== '') {
                                        element.crossOrigin = scripts[s].crossorigin;
                                    }
                                    if (scripts[s].type !== '') {
                                        element.type = scripts[s].type;
                                    }
                                    if (scripts[s].dataAttributes !== null) {
                                        for (var key in scripts[s].dataAttributes) {
                                            element.setAttribute(key, scripts[s].dataAttributes[key]);
                                        }
                                    }
                                    if (scripts[s].location === 'body') {
                                        document.body.appendChild(element);
                                        return false;  // return false to bypass default DOM insertion mechanism
                                    }
                                }
                            }
                        }
                    })
                    .then(function () { resolve(true) })
                    .catch(function (pathsNotFound) { reject(false) });
                }
            }));
        }
        if (promises.length !== 0) {
            await Promise.all(promises);
        }
    },
    getAbsoluteUrl: function (url) {
        var a = document.createElement('a');
        getAbsoluteUrl = function (url) {
            a.href = url;
            return a.href;
        }
        return getAbsoluteUrl(url);
    },
    removeElementsById: function (prefix, first, last) {
        var elements = document.querySelectorAll('[id^=' + prefix + ']');
        for (var i = elements.length - 1; i >= 0; i--) {
            var element = elements[i];
            if (element.id.startsWith(prefix) && (first === '' || element.id >= first) && (last === '' || element.id <= last)) {
                element.parentNode.removeChild(element);
            }
        }
    },
    getElementByName: function (name) {
        var elements = document.getElementsByName(name);
        if (elements.length) {
            return elements[0].value;
        } else {
            return "";
        }
    },
    submitForm: function (path, fields) {
        const form = document.createElement('form');
        form.method = 'post';
        form.action = path;

        for (const key in fields) {
            if (fields.hasOwnProperty(key)) {
                const hiddenField = document.createElement('input');
                hiddenField.type = 'hidden';
                hiddenField.name = key;
                hiddenField.value = fields[key];
                form.appendChild(hiddenField);
            }
        }

        document.body.appendChild(form);
        form.submit();
    },
    getFiles: function (id) {
        var files = [];
        var fileinput = document.getElementById(id);
        if (fileinput !== null) {
            for (var i = 0; i < fileinput.files.length; i++) {
                files.push(fileinput.files[i].name + ":" + fileinput.files[i].size);
            }
        }
        return files;
    },
    uploadFiles: function (posturl, folder, id, antiforgerytoken, jwt, maxChunkSizeMB, maxConcurrentUploads) {
        var fileinput = document.getElementById('FileInput_' + id);
        var progressinfo = document.getElementById('ProgressInfo_' + id);
        var progressbar = document.getElementById('ProgressBar_' + id);

        if (progressinfo !== null && progressbar !== null) {
            progressinfo.setAttribute("style", "display: inline;");
            progressinfo.innerHTML = '';
            progressbar.setAttribute("style", "width: 100%; display: inline;");
            progressbar.value = 0;
        }

        var files = fileinput.files;
        var totalSize = 0;
        for (var i = 0; i < files.length; i++) {
            totalSize = totalSize + files[i].size;
        }

        maxChunkSizeMB = Math.ceil(maxChunkSizeMB);
        if (maxChunkSizeMB < 1) {
            maxChunkSizeMB = 1;
        }
        else if (maxChunkSizeMB > 50) {
            maxChunkSizeMB = 50;
        }

        var bufferChunkSize = maxChunkSizeMB * (1024 * 1024);
        var uploadedSize = 0;

        maxConcurrentUploads = Math.ceil(maxConcurrentUploads);
        var hasConcurrencyLimit = maxConcurrentUploads > 0;
        var uploadQueue = [];
        var activeUploads = 0;

        for (var i = 0; i < files.length; i++) {
            var fileChunk = [];
            var file = files[i];
            var fileStreamPos = 0;
            var endPos = bufferChunkSize;

            while (fileStreamPos < file.size) {
                fileChunk.push(file.slice(fileStreamPos, endPos));
                fileStreamPos = endPos;
                endPos = fileStreamPos + bufferChunkSize;
            }

            var totalParts = fileChunk.length;
            var partCount = 0;

            while (chunk = fileChunk.shift()) {
                partCount++;
                var fileName = file.name + ".part_" + partCount.toString().padStart(3, '0') + "_" + totalParts.toString().padStart(3, '0');

                var data = new FormData();
                data.append('__RequestVerificationToken', antiforgerytoken);
                data.append('folder', folder);
                data.append('formfile', chunk, fileName);
                var request = new XMLHttpRequest();
                request.open('POST', posturl, true);
                if (jwt !== "") {
                    request.setRequestHeader('Authorization', 'Bearer ' + jwt);
                    request.withCredentials = true;
                }
                request.upload.onloadstart = function (e) {
                    if (progressinfo !== null && progressbar !== null && progressinfo.innerHTML === '') {
                        if (files.length === 1) {
                            progressinfo.innerHTML = file.name;
                        }
                        else {
                            progressinfo.innerHTML = file.name + ", ...";
                        }
                    }
                };
                request.upload.onprogress = function (e) {
                    if (progressinfo !== null && progressbar !== null) {
                        var percent = Math.ceil(((uploadedSize + e.loaded) / totalSize) * 100);
                        progressbar.value = (percent / 100);
                    }
                };
                request.upload.onloadend = function (e) {
                    if (hasConcurrencyLimit) {
                        activeUploads--;
                        processUploads();
                    }

                    if (progressinfo !== null && progressbar !== null) {
                        uploadedSize = uploadedSize + e.total;
                        var percent = Math.ceil((uploadedSize / totalSize) * 100);
                        progressbar.value = (percent / 100);
                    }
                };
                request.upload.onerror = function () {
                    if (hasConcurrencyLimit) {
                        activeUploads--;
                        processUploads();
                    }

                    if (progressinfo !== null && progressbar !== null) {
                        if (files.length === 1) {
                            progressinfo.innerHTML = file.name + ' Error: ' + request.statusText;
                        }
                        else {
                            progressinfo.innerHTML = ' Error: ' + request.statusText;
                        }
                    }
                };

                if (hasConcurrencyLimit) {
                    uploadQueue.push({ data, request });
                    processUploads();
                }
                else {
                    request.send(data);
                }
            }

            if (i === files.length - 1) {
                fileinput.value = '';
            }
        }

        function processUploads() {
            if (uploadQueue.length === 0 || activeUploads >= maxConcurrentUploads) {
                return;
            }

            while (activeUploads < maxConcurrentUploads && uploadQueue.length > 0) {
                activeUploads++;

                let { data, request } = uploadQueue.shift();
                request.send(data);
            }
        }
    },
    refreshBrowser: function (verify, wait) {
        async function attemptReload (verify) {
            if (verify) {
                await fetch('');
            }
            window.location.reload();
        }
        attemptReload(verify);
        setInterval(attemptReload, wait * 1000);
    },
    redirectBrowser: function (url, wait) {
        setInterval(function () {
            window.location.href = url;
        }, wait * 1000);
    },
    formValid: function (formRef) {
        return formRef.checkValidity();
    },
    setElementAttribute: function (id, attribute, value) {
        var element = document.getElementById(id);
        if (element !== null) {
            element.setAttribute(attribute, value);
        }
    },
    scrollTo: function (top, left, behavior) {
        const modal = document.querySelector('.modal');
        if (modal) {
            modal.scrollTo({
                top: top,
                left: left,
                behavior: behavior
            });
        } else {
            window.scrollTo({
                top: top,
                left: left,
                behavior: behavior
            });
        }
    },
    scrollToId: function (id) {
        var element = document.getElementById(id);
        if (element instanceof HTMLElement) {
            element.scrollIntoView({
                behavior: "smooth",
                block: "start",
                inline: "nearest"
            });
        }
    },
    getCaretPosition: function (id) {
        var element = document.getElementById(id);
        return element.selectionStart;
    },
    manageIndexedDBItems: async function (action, key, value) {
        var idb = indexedDB.open("oqtane", 1);

        idb.onupgradeneeded = function () {
            let db = idb.result;
            db.createObjectStore("items");
        }

        if (action.startsWith("get")) {
            let request = new Promise((resolve) => {
                idb.onsuccess = function () {
                    let transaction = idb.result.transaction("items", "readonly");
                    let collection = transaction.objectStore("items");
                    let result;
                    if (action === "get") {
                        result = collection.get(key);
                    }
                    if (action === "getallkeys") {
                        result = collection.getAllKeys();
                    }

                    result.onsuccess = function (e) {
                        resolve(result.result);
                    }
                }
            });

            let result = await request;

            return result;
        }
        else {
            idb.onsuccess = function () {
                let transaction = idb.result.transaction("items", "readwrite");
                let collection = transaction.objectStore("items");
                if (action === "put") {
                    collection.put(value, key);
                }
                if (action === "delete") {
                    collection.delete(key);
                }
            }
        }
    }
};
