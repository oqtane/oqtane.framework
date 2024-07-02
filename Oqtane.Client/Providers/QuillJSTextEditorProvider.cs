using Oqtane.Interfaces;

namespace Oqtane.Providers
{
    public class QuillJSTextEditorProvider : ITextEditorProvider
    {
        public string Name => "QuillJS";

        public string EditorType => "Oqtane.Modules.Controls.QuillJSTextEditor, Oqtane.Client";
    }
}
