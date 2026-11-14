using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
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
    private IServiceScope? _appScope;

    public IServiceProvider Services => _appScope!.ServiceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += (_, args) =>
        {
            ShowError(args.Exception);
            args.Handled = true;
        };
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            ShowError(args.Exception);
            args.SetObserved();
        };
        base.OnStartup(e);
    }

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        var builder = Host.CreateApplicationBuilder();

        // ---- Data ----
        builder.Services.AddDbContext<AppDbContext>(o =>
            o.UseSqlite(DatabasePath.GetConnectionString()));

        builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();

        // ---- Core services (Scoped, зависят от репозиториев) ----
        builder.Services.AddScoped<IDocumentService, DocumentService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        // builder.Services.AddScoped<ISettingsService, SettingsService>(); // когда будет реализация

        // ---- Singletons ----
        builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<IDocumentFormatService, DocumentFormatService>();
        builder.Services.AddSingleton<IFileDialogService, FileDialogService>();
        builder.Services.AddSingleton<ImageInserter>();

        // ---- ViewModels / Views (Transient!) ----
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginWindow>();
        builder.Services.AddTransient<EditorViewModel>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainWindow>();

        _host = builder.Build();
        await _host.StartAsync();

        // Корневой scope живёт весь жизненный цикл приложения.
        _appScope = _host.Services.CreateScope();
        var sp = _appScope.ServiceProvider;

        // Миграции — внутри scope, где живёт AppDbContext
        var db = sp.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        // Логин
        var loginWindow = sp.GetRequiredService<LoginWindow>();
        if (loginWindow.ShowDialog() != true)
        {
            Shutdown();
            return;
        }

        // Главное окно
        sp.GetRequiredService<MainWindow>().Show();
    }

    private async void Application_Exit(object sender, ExitEventArgs e)
    {
        _appScope?.Dispose();
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }

    private static void ShowError(Exception ex)
    {
        try
        {
            var logFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TextEditor");
            Directory.CreateDirectory(logFolder);
            File.AppendAllText(Path.Combine(logFolder, "error.log"),
                $"[{DateTimeOffset.Now:O}] {ex}\n\n");
        }
        catch { /* логирование не должно ронять приложение */ }

        MessageBox.Show(
            $"Произошла непредвиденная ошибка:\n\n{ex.Message}\n\nТип: {ex.GetType().Name}",
            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}