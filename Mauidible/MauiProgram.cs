using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;

using Mauidible.Domain;
using Mauidible.Services;

namespace Mauidible
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
            // Register the FolderPicker as a singleton
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("FluentIcons.ttf", "FluentIcons");
                });

            builder.Services.AddSingleton(FolderPicker.Default);
            builder.Services.AddDbContext<AppDbContext>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<Player>();
            builder.Services.AddSingleton<IMetadataService, MetadataService>();

            using var context = new AppDbContext();
            context.Database.EnsureCreated();

            return builder.Build();
        }
    }
}
