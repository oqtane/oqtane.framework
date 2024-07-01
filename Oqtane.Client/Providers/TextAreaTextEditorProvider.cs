using Oqtane.Interfaces;

namespace Oqtane.Providers
{
    public class TextAreaTextEditorProvider : ITextEditorProvider
    {
        public string Name => "TextArea";

        public string EditorType => "Oqtane.Modules.Controls.TextAreaTextEditor, Oqtane.Client";

        public string SettingsType => string.Empty;
    }
}
