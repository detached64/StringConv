using StringConv.Models.Settings;
using System;
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace StringConv.Services;

internal sealed class SettingsService : ISettingsService
{
    private static readonly string DefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name, "settings.json");

    private static readonly JsonSerializerOptions _options = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true
    };

    private static readonly AppSettingsSerializationContext _context = new(_options);

    public AppSettings Settings { get; }

    public SettingsService()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DefaultPath));
        if (!File.Exists(DefaultPath))
        {
            Settings = new();
        }
        else
        {
            try
            {
                string json = File.ReadAllText(DefaultPath);
                Settings = JsonSerializer.Deserialize(json, _context.AppSettings) ?? new();
            }
            catch
            {
                Settings = new();
            }
        }
    }

    public void SaveSettings()
    {
        string json = JsonSerializer.Serialize(Settings, _context.AppSettings);
        File.WriteAllText(DefaultPath, json);
    }
}
