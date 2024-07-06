using Oqtane.Interfaces;

namespace Oqtane.Providers
{
    public class QuillTextEditorProvider : ITextEditorProvider
    {
        public string Name => "Quill";

        public string EditorType => "Oqtane.Modules.Controls.QuillTextEditor, Oqtane.Client";
    }
}
