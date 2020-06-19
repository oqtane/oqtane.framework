using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Oqtane.Modules.Controls
{
    public class RichTextEditorInterop
    {
        private readonly IJSRuntime _jsRuntime;

        public RichTextEditorInterop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task CreateEditor(
            ElementReference quillElement,
            ElementReference toolbar,
            bool readOnly,
            string placeholder,
            string theme,
            string debugLevel)
        {
            try
            {
                await _jsRuntime.InvokeAsync<object>(
                    "Oqtane.RichTextEditor.createQuill",
                    quillElement, toolbar, readOnly, placeholder, theme, debugLevel);
                return;
            }
            catch
            {
                // handle exception
            }
        }

        public ValueTask<string> GetText(ElementReference quillElement)
        {
            try
            {
                return _jsRuntime.InvokeAsync<string>(
                    "Oqtane.RichTextEditor.getQuillText",
                    quillElement);
            }
            catch
            {
                return new ValueTask<string>(Task.FromResult(string.Empty));
            }
        }

        public ValueTask<string> GetHtml(ElementReference quillElement)
        {
            try
            {
                return _jsRuntime.InvokeAsync<string>(
                    "Oqtane.RichTextEditor.getQuillHTML",
                    quillElement);
            }
            catch
            {
                return new ValueTask<string>(Task.FromResult(string.Empty));
            }
        }

        public ValueTask<string> GetContent(ElementReference quillElement)
        {
            try
            {
                return _jsRuntime.InvokeAsync<string>(
                    "Oqtane.RichTextEditor.getQuillContent",
                    quillElement);
            }
            catch
            {
                return new ValueTask<string>(Task.FromResult(string.Empty));
            }
        }

        public Task LoadEditorContent(ElementReference quillElement, string content)
        {
            try
            {
                _jsRuntime.InvokeAsync<object>(
                    "Oqtane.RichTextEditor.loadQuillContent",
                    quillElement, content);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task EnableEditor(ElementReference quillElement, bool mode)
        {
            try
            {
                _jsRuntime.InvokeAsync<object>(
                    "Oqtane.RichTextEditor.enableQuillEditor", quillElement, mode);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        public Task InsertImage(ElementReference quillElement, string imageUrl)
        {
            try
            {
                _jsRuntime.InvokeAsync<object>(
                    "Oqtane.RichTextEditor.insertQuillImage",
                    quillElement, imageUrl);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }
    }
}
