using System.Linq;
using System.Text;
using System.Windows;

namespace StringConv
{
    public partial class EncodingOptions : Window
    {
        public EncodingOptions()
        {
            InitializeComponent();
            DataContext = EncodingsViewModel.Instance;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddEncodingBox addEncodingBox = new AddEncodingBox();
            addEncodingBox.Owner = this;
            if (addEncodingBox.ShowDialog() == true)
            {
                EncodingsViewModel.Instance.AddAll(addEncodingBox.DataEncoding.SelectedItems.Cast<Encoding>().ToList());
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataEncoding.SelectedItems != null)
            {
                EncodingsViewModel.Instance.RemoveAll(this.DataEncoding.SelectedItems.Cast<Encoding>().ToList());
            }
        }

        private void BtnRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            EncodingsViewModel.Instance.Clear();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
