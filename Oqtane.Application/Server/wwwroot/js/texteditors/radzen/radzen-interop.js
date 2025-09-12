var Oqtane = Oqtane || {};

Oqtane.RadzenTextEditor = {
    initialize: function (editor) {
        if (typeof Radzen.openPopup === "function" && Radzen.openPopup !== Oqtane.RadzenTextEditor.openPopup) {
            Oqtane.RadzenTextEditor.radzenOpenPopup = Radzen.openPopup;
            Radzen.openPopup = Oqtane.RadzenTextEditor.openPopup;
        }
    },
    openPopup: function () {
        Oqtane.RadzenTextEditor.radzenOpenPopup.apply(this, arguments);
        var id = arguments[1];
        var popup = document.getElementById(id);
        if (popup) {
            Oqtane.RadzenTextEditor.updateButtonStyles(popup);
        }
    },
    setBackgroundColor: function (editor, color) {
        editor.getElementsByClassName("rz-html-editor-content")[0].style.backgroundColor = color;
    },
    updateDialogLayout: function (editor) {
        var dialogs = editor.parentElement.getElementsByClassName('rz-dialog-wrapper');
        for (var dialog of dialogs) {
            document.body.appendChild(dialog);
            dialog.classList.add('rz-editor-dialog-wrapper', 'text-dark');

            this.updateButtonStyles(dialog);
        }
    },
    updateButtonStyles: function (parent) {
        var primaryBtns = parent.getElementsByClassName('rz-primary');
        if (primaryBtns) {
            for (var btn of primaryBtns) {
                btn.classList.remove('rz-button', 'rz-primary');
                btn.classList.add('btn', 'btn-primary');
            }
        }

        var secondaryBtns = parent.getElementsByClassName('rz-secondary');
        if (secondaryBtns) {
            for (var btn of secondaryBtns) {
                btn.classList.remove('rz-button', 'rz-secondary');
                btn.classList.add('btn', 'btn-secondary');
            }
        }
    }
}