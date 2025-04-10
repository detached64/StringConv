using System.Windows;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace StringConv
{
    public partial class AddEncodingBox : Window
    {
        private static List<Encoding> SupportedEncodings { get; } =
            Encoding.GetEncodings()
                .Select(e => e.GetEncoding())
                .ToList();

        public AddEncodingBox()
        {
            InitializeComponent();
            this.DataEncoding.ItemsSource = SupportedEncodings;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
