using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Data;

public static class DatabasePath
{
    public static string Get()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TextEditor");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "app.db");
    }

    public static string GetConnectionString()
        => $"Data Source={Get()}";
}