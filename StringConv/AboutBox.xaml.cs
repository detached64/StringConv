using System.Reflection;
using System.Windows;

namespace StringConv
{
    /// <summary>
    /// AboutBox.xaml 的交互逻辑
    /// </summary>
    public partial class AboutBox : Window
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.TextName.Text = $"{Application.ResourceAssembly.GetName().Name} {Application.ResourceAssembly.GetName().Version}";
            this.TextCopyright.Text = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
            this.TextLink.Text = "https://github.com/detached64/StringConv";
        }
    }
}
