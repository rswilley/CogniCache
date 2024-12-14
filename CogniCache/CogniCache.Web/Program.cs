using Blazorise;
using Blazorise.FluentUI2;
using Blazorise.Icons.FluentUI;
using CogniCache.Shared.Services;
using CogniCache.Web.Components;
using CogniCache.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Blazorise/FluentUI2
builder.Services
    .AddBlazorise()
    .AddFluentUI2Providers()
    .AddFluentUIIcons();

// Add device-specific services used by the CogniCache.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(CogniCache.Shared._Imports).Assembly);

app.Run();
