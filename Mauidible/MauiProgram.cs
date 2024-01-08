using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;

using Mauidible.Domain;

using Microsoft.Extensions.Logging;

namespace Mauidible
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp ()
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
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton(FolderPicker.Default);
            builder.Services.AddDbContext<AppDbContext>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<Player>();

            var context = new AppDbContext();
            context.Database.EnsureCreated();
            context.Dispose();

            return builder.Build();
        }
    }
}
