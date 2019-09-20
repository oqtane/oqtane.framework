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
    getElementByName: function (name) {
        var elements = document.getElementsByName(name);
        if (elements.length) {
            return elements[0].value;
        } else {
            return "";
        }
    },
    addCSS: function (fileName) {
        var head = document.head;
        var link = document.createElement("link");

        link.type = "text/css";
        link.rel = "stylesheet";
        link.href = fileName;

        head.appendChild(link);
    },
    removeCSS: function (filePattern) {
        var head = document.head;
        var links = document.getElementsByTagName("link");
        for (var i = 0; i < links.length; i++) {
            var link = links[i];
            if (link.rel === 'stylesheet' && link.href.includes(filePattern)) {
                head.removeChild(link);
            }
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
    uploadFiles: function (posturl, folder, name) {
        var files = document.getElementById(name + 'FileInput').files;
        var progressinfo = document.getElementById(name + 'ProgressInfo');
        var progressbar = document.getElementById(name + 'ProgressBar');
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
