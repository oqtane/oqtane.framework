using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.UI
{
    public class TabStripInterop
    {
        private readonly IJSRuntime _jsRuntime;

        public TabStripInterop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task UpdateView(string id)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("Oqtane.Controls.TabStrip.Interop.updateView",id);
            }
            catch
            {
                //do nothing here.
            }
        }
    }
}
