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
        builder.Services.AddDbContext<AppDbContext>(o =>
            o.UseSqlite("Data Source=app.db"));
        builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();

        // --- Core services (реализации — TODO в Sprint 1) ---
        // builder.Services.AddScoped<IDocumentService, DocumentService>();
        // builder.Services.AddScoped<ISettingsService, SettingsService>();

        // --- WPF services ---
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

        _host.Services.GetRequiredService<MainWindow>().Show();
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