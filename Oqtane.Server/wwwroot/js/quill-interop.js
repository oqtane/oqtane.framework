var Oqtane = Oqtane || {};

Oqtane.RichTextEditor = {
    createQuill: async function (
        quillElement, toolBar, readOnly,
        placeholder, theme, debugLevel) {

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
