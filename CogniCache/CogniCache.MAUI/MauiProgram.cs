﻿using Blazorise;
using Blazorise.FluentUI2;
using Blazorise.Icons.FluentUI;
using CogniCache.DependencyResolver;
using CogniCache.Services;
using CogniCache.Shared.Services;
using Microsoft.Extensions.Logging;

namespace CogniCache
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Add device-specific services used by the CogniCache.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            builder.Services.AddShared();
            builder.Services.AddApplication();
            builder.Services.AddDomain();
            builder.Services.AddInfrastructure();

            builder.Services.AddMauiBlazorWebView();

            // Add Blazorise/FluentUI2
            builder.Services
            .AddBlazorise()
            .AddFluentUI2Providers()
            .AddFluentUIIcons();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
