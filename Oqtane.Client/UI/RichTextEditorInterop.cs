using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Oqtane.UI
{
    public static class RichTextEditorInterop
    {
        internal static ValueTask<object> CreateEditor(
            IJSRuntime jsRuntime,
            ElementReference quillElement,
            ElementReference toolbar,
            bool readOnly,
            string placeholder,
            string theme,
            string debugLevel)
        {
            return jsRuntime.InvokeAsync<object>(
                "interop.createQuill",
                quillElement, toolbar, readOnly,
                placeholder, theme, debugLevel);
        }

        internal static ValueTask<string> GetText(
            IJSRuntime jsRuntime,
            ElementReference quillElement)
        {
            return jsRuntime.InvokeAsync<string>(
                "interop.getQuillText",
                quillElement);
        }

        internal static ValueTask<string> GetHtml(
            IJSRuntime jsRuntime,
            ElementReference quillElement)
        {
            return jsRuntime.InvokeAsync<string>(
                "interop.getQuillHTML",
                quillElement);
        }

        internal static ValueTask<string> GetContent(
            IJSRuntime jsRuntime,
            ElementReference quillElement)
        {
            return jsRuntime.InvokeAsync<string>(
                "interop.getQuillContent",
                quillElement);
        }

        internal static ValueTask<object> LoadEditorContent(
            IJSRuntime jsRuntime,
            ElementReference quillElement,
            string content)
        {
            return jsRuntime.InvokeAsync<object>(
                "interop.loadQuillContent",
                quillElement, content);
        }

        internal static ValueTask<object> EnableEditor(
            IJSRuntime jsRuntime,
            ElementReference quillElement,
            bool mode)
        {
            return jsRuntime.InvokeAsync<object>(
                "interop.enableQuillEditor", quillElement, mode);
        }

        internal static ValueTask<object> InsertImage(
            IJSRuntime jsRuntime,
            ElementReference quillElement,
            string imageUrl)
        {
            return jsRuntime.InvokeAsync<object>(
                "interop.insertQuillImage",
                quillElement, imageUrl);
        }
    }
}
