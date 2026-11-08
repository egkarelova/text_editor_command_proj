using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TextEditor.App.Services;
using TextEditor.App.ViewModels;
using TextEditor.App.Views;
using TextEditor.Core.Repositories;
using TextEditor.Core.Services;
using TextEditor.Data;
using TextEditor.Data.Repositories;


namespace TextEditor.App;

public partial class App : Application
{
    private IHost? _host;

    public IServiceProvider Services => _host!.Services;

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        var builder = Host.CreateApplicationBuilder();

        // --- Data ---
        builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(DatabasePath.GetConnectionString()));
        builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();

        // --- Core services (реализации — TODO в Sprint 1) ---
        // builder.Services.AddScoped<IDocumentService, DocumentService>();
        // builder.Services.AddScoped<ISettingsService, SettingsService>();

        // --- WPF services ---
        builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<IDocumentFormatService, DocumentFormatService>();
        builder.Services.AddSingleton<ImageInserter>();

        // --- ViewModels / Views ---
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<EditorViewModel>();
        builder.Services.AddSingleton<MainWindow>();

        _host = builder.Build();
        await _host.StartAsync();

        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        // Инициализация пользователя — после миграций
        var currentUser = _host.Services.GetRequiredService<ICurrentUserService>();
        await currentUser.InitializeAsync();

        _host.Services.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        // UI-поток
        DispatcherUnhandledException += (_, args) =>
        {
            ShowError(args.Exception);
            args.Handled = true;
        };

        // Фоновые задачи (Task)
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            ShowError(args.Exception);
            args.SetObserved();
        };

        base.OnStartup(e);
    }

    private static void ShowError(Exception ex)
    {
        MessageBox.Show(
            $"Произошла непредвиденная ошибка:\n\n{ex.Message}\n\n" +
            $"Подробности: {ex.GetType().Name}",
            "Ошибка",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TextEditor", "error.log");
        File.AppendAllText(logPath, $"[{DateTime.Now:O}] {ex}\n\n");
    }

    private async void Application_Exit(object sender, ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }
}