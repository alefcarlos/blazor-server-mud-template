using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MyAppWithMud.Components.BrowserTime;

namespace MyAppWithMud.Components;
public sealed class InitializeTimeZone : ComponentBase
{
    [Inject] public BrowserTimeProvider TimeProvider { get; set; } = default!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                var timeZone = await JSRuntime.InvokeAsync<string>("getBrowserTimeZone");
                TimeProvider.SetBrowserTimeZone(timeZone);
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }
}
