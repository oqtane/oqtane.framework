var Oqtane = Oqtane || {};

Oqtane.RichTextEditor = {
    createQuill: async function (
        quillElement, toolBar, readOnly,
        placeholder, theme, debugLevel) {

        const loadQuill = loadjs(['js/quill1.3.6.min.js', 'js/quill-blot-formatter.min.js'], 'Quill',
            { async: true, returnPromise: true })
            .then(function () { /* foo.js & bar.js loaded */
                Quill.register('modules/blotFormatter', QuillBlotFormatter.default);

                var options = {
                    debug: debugLevel,
                    modules: {
                        toolbar: toolBar,
                        blotFormatter: {}
                    },
                    placeholder: placeholder,
                    readOnly: readOnly,
                    theme: theme
                };

                new Quill(quillElement, options);
            })
            .catch(function (pathsNotFound) { /* at least one didn't load */ });

        await loadQuill;
    },
    getQuillContent: function (editorElement) {
        return JSON.stringify(editorElement.__quill.getContents());
    },
    getQuillText: function (editorElement) {
        return editorElement.__quill.getText();
    },
    getQuillHTML: function (editorElement) {
        return editorElement.__quill.root.innerHTML;
    },
    loadQuillContent: function (editorElement, editorContent) {
        return editorElement.__quill.root.innerHTML = editorContent;
    },
    enableQuillEditor: function (editorElement, mode) {
        editorElement.__quill.enable(mode);
    },
    insertQuillImage: function (quillElement, imageURL) {
        var Delta = Quill.import('delta');
        editorIndex = 0;

        if (quillElement.__quill.getSelection() !== null) {
            editorIndex = quillElement.__quill.getSelection().index;
        }

        return quillElement.__quill.updateContents(
            new Delta()
                .retain(editorIndex)
                .insert({ image: imageURL },
                    { alt: imageURL }));
    }
};
