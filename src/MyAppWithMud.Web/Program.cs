using MyAppWithMud.Components;
using MyAppWithMud.Web;
using MyAppWithMud.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.ConfigureServices();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    app.UseHttpsRedirection();
}

//Descomentar para usar auth/authz
//app.UseAuthentication();
//app.UseAuthorization();

app.UseAntiforgery();

app.UseRequestLocalization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(IComponentsMarker).Assembly)
    ;

//Descomentar para usar auth/authz
//app.MapLoginAndLogoutDefaults();

app.MapDefaultEndpoints();

await app.RunAsync();
