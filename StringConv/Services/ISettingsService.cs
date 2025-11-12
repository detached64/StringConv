using StringConv.Models.Settings;

namespace StringConv.Services;

internal interface ISettingsService
{
    AppSettings Settings { get; }
    void SaveSettings();
}
