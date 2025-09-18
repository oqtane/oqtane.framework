using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Oqtane.Modules.Controls
{
    public class RadzenTextEditorInterop
    {
        private readonly IJSRuntime _jsRuntime;

        public RadzenTextEditorInterop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public Task Initialize(ElementReference editor)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync("Oqtane.RadzenTextEditor.initialize", editor);
            }
            catch
            {

            }

            return Task.CompletedTask;
        }

        public Task SetBackgroundColor(ElementReference editor, string color)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync(
                    "Oqtane.RadzenTextEditor.setBackgroundColor",
                    editor, color);
            }
            catch
            {
                
            }

            return Task.CompletedTask;
        }

        public Task UpdateDialogLayout(ElementReference editor)
        {
            try
            {
                _jsRuntime.InvokeVoidAsync("Oqtane.RadzenTextEditor.updateDialogLayout", editor);
            }
            catch
            {

            }

            return Task.CompletedTask;
        }
    }
}
