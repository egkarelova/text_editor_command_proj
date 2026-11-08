using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Data;

/// Единая точка вычисления пути к файлу БД.
/// Используется и в App.xaml.cs, и в DesignTimeDbContextFactory,
/// чтобы миграции и приложение работали с одним и тем же файлом.

public static class DatabasePath
{
    public static string Get()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TextEditor");
        Directory.CreateDirectory(folder); // идемпотентно
        return Path.Combine(folder, "app.db");
    }

    public static string GetConnectionString()
        => $"Data Source={Get()}";
}