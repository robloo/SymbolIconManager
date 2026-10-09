using Avalonia;
using Avalonia.Metadata;

[assembly: XmlnsDefinition("https://github.com/avaloniaui", "IconManager")]
[assembly: XmlnsDefinition("https://github.com/avaloniaui", "IconManager.ViewModels")]
[assembly: XmlnsDefinition("https://github.com/avaloniaui", "IconManager.Views")]

namespace IconManager
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
