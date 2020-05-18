window.interop = {
    setCookie: function (name, value, days) {
        var d = new Date();
        d.setTime(d.getTime() + (days * 24 * 60 * 60 * 1000));
        var expires = "expires=" + d.toUTCString();
        document.cookie = name + "=" + value + ";" + expires + ";path=/";
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
        var meta;
        if (id !== "") {
            meta = document.getElementById(id);
        }
        else {
            meta = document.querySelector("meta[" + attribute + "=\"" + CSS.escape(name) + "\"]");
        }
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
            if (meta.content !== content) {
                meta.setAttribute("content", content);
            }
        }
    },
    includeLink: function (id, rel, url, type, integrity, crossorigin) {
        var link;
        if (id !== "") {
            link = document.getElementById(id);
        }
        else {
            link = document.querySelector("link[href=\"" + CSS.escape(url) + "\"]");
        }
        if (link === null) {
            link = document.createElement("link");
            if (id !== "") {
                link.id = id;
            }
            link.rel = rel;
            if (type !== "") {
                link.type = type;
            }
            link.href = url;
            if (integrity !== "") {
                link.integrity = integrity;
            }
            if (crossorigin !== "") {
                link.crossOrigin = crossorigin;
            }
            document.head.appendChild(link);
        }
        else {
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
            if (link.href !== url) {
                link.removeAttribute('integrity');
                link.removeAttribute('crossorigin');
                link.setAttribute('href', url);
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
    includeScript: function (id, src, content, location, integrity, crossorigin) {
        var script;
        if (id !== "") {
            script = document.getElementById(id);
        }
        if (script === null) {
            script = document.createElement("script");
            if (id !== "") {
                script.id = id;
            }
            if (src !== "") {
                script.src = src;
                if (integrity !== "") {
                    script.integrity = integrity;
                }
                if (crossorigin !== "") {
                    script.crossorigin = crossorigin;
                }
            }
            else {
                script.innerHTML = content;
            }
            if (location === 'head') {
                document.head.appendChild(script);
            }
            if (location === 'body') {
                document.body.appendChild(script);
            }
        }
        else {
            if (src !== "") {
                if (script.src !== src) {
                    script.removeAttribute('integrity');
                    script.removeAttribute('crossorigin');
                    script.src = src;
                }
                if (integrity !== "") {
                    if (script.integrity !== integrity) {
                        script.setAttribute('integrity', integrity);
                    }
                } else {
                    script.removeAttribute('integrity');
                }
                if (crossorigin !== "") {
                    if (script.crossOrigin !== crossorigin) {
                        script.setAttribute('crossorigin', crossorigin);
                    }
                } else {
                    script.removeAttribute('crossorigin');
                }
            }
            else {
                if (script.innerHTML !== content) {
                    script.innerHTML = content;
                }
            }
        }
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
                files.push(fileinput.files[i].name);
            }
        }
        return files;
    },
    uploadFiles: function (posturl, folder, id) {
        var files = document.getElementById(id + 'FileInput').files;
        var progressinfo = document.getElementById(id + 'ProgressInfo');
        var progressbar = document.getElementById(id + 'ProgressBar');
        var filename = '';

        for (var i = 0; i < files.length; i++) {
            var FileChunk = [];
            var file = files[i];
            var MaxFileSizeMB = 1;
            var BufferChunkSize = MaxFileSizeMB * (1024 * 1024);
            var FileStreamPos = 0;
            var EndPos = BufferChunkSize;
            var Size = file.size;

            progressbar.setAttribute("style", "visibility: visible;");

            if (files.length > 1) {
                filename = file.name;
            }

            while (FileStreamPos < Size) {
                FileChunk.push(file.slice(FileStreamPos, EndPos));
                FileStreamPos = EndPos;
                EndPos = FileStreamPos + BufferChunkSize;
            }

            var TotalParts = FileChunk.length;
            var PartCount = 0;

            while (Chunk = FileChunk.shift()) {
                PartCount++;
                var FileName = file.name + ".part_" + PartCount + "_" + TotalParts;

                var data = new FormData();
                data.append('folder', folder);
                data.append('file', Chunk, FileName);
                var request = new XMLHttpRequest();
                request.open('POST', posturl, true);
                request.upload.onloadstart = function (e) {
                    progressbar.value = 0;
                    progressinfo.innerHTML = filename + ' 0%';
                };
                request.upload.onprogress = function (e) {
                    var percent = Math.ceil((e.loaded / e.total) * 100);
                    progressbar.value = (percent / 100);
                    progressinfo.innerHTML = filename + '[' + PartCount + '] ' + percent + '%';
                };
                request.upload.onloadend = function (e) {
                    progressbar.value = 1;
                    progressinfo.innerHTML = filename + ' 100%';
                };
                request.send(data);
            }
        }
    }
};
