using System.Configuration;
using System.Diagnostics;

namespace StringConv
{
    public class Settings : ApplicationSettingsBase
    {
        public static Settings Default { get; } = (Settings)Synchronized(new Settings());

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("0")]
        public int SelectedEncodingIndex
        {
            get => (int)this[nameof(SelectedEncodingIndex)];
            set => this[nameof(SelectedEncodingIndex)] = value;
        }
    }
}
