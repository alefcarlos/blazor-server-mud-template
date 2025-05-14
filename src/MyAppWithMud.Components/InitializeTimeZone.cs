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
                await using var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/timezone.js");
                var timeZone = await module.InvokeAsync<string>("getBrowserTimeZone");
                TimeProvider.SetBrowserTimeZone(timeZone);
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }
}
