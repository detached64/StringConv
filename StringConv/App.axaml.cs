using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using StringConv.Models.Converters;
using StringConv.Services;
using StringConv.ViewModels;
using StringConv.Views;
using System;
using System.Text;

namespace StringConv;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public static TopLevel Top { get; private set; }

    public override void Initialize()
    {
        ServiceProvider = ConfigureService();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        RegisterEncodings();
        MainView mainView = ServiceProvider.GetRequiredService<MainView>();
        mainView.DataContext = ServiceProvider.GetRequiredService<MainViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = mainView;
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            desktop.Exit += (_, _) => ServiceProvider.GetRequiredService<ISettingsService>().SaveSettings();
            Top = TopLevel.GetTopLevel(desktop.MainWindow);
        }
        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureService()
    {
        ServiceCollection services = new();
        // Views
        services.AddSingleton<MainView>();
        services.AddTransient<AboutView>();
        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<AboutViewModel>();
        // Services
        services.AddSingleton<IShowDialogService, ShowDialogService>();
        services.AddSingleton<ISettingsService, SettingsService>();

        return services.BuildServiceProvider();
    }

    private static void RegisterEncodings()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        foreach (EncodingInfo encInfo in Encoding.GetEncodings())
        {
            StringConverterProvider.Converters.Add(new CharEncodingConverter(encInfo.GetEncoding()));
        }
    }
}
