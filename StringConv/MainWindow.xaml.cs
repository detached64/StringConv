using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StringConv
{
    public sealed partial class MainWindow : Window
    {
        private bool IsUpdating = false;
        public byte[] Input = null;

        public MainWindow()
        {
            InitializeComponent();
            AddCustomEncodings();
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsUpdating)
            {
                return;
            }
            IsUpdating = true;
            TextBox textBox = (TextBox)sender;
            string input = textBox.Text;
            Input = ProcessString(input, textBox);
            UpdateStrings(textBox);
            UpdateHex(textBox);
            UpdateFormattedHex(textBox);
            UpdateByteCount();
            IsUpdating = false;
        }

        private byte[] ProcessString(string input, TextBox sender)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            if (sender.Name == "TextHex")
            {
                return HexStringToByteArray(new string(input.Where(c => Uri.IsHexDigit(c)).ToArray()).ToUpper());
            }
            else
            {
                return Encoding.GetEncoding(int.Parse(sender.Tag.ToString())).GetBytes(input);
            }
        }

        private void UpdateByteCount()
        {
            this.TextByteCount.Text = Input == null ? (this.TextHex.Text.Length == 0 ? "0" : "Parse error") : Input.Length.ToString();
        }

        private void UpdateStrings(TextBox sender)
        {
            foreach (TextBox box in this.GridString.Children.OfType<TextBox>())
            {
                if (box == sender)
                {
                    continue;
                }
                if (Input == null || Input.Length == 0 || box.Tag == null)
                {
                    box.Text = string.Empty;
                }
                else
                {
                    box.Text = Encoding.GetEncoding(int.Parse(box.Tag.ToString())).GetString(Input);
                }
            }
        }

        private void UpdateHex(TextBox sender)
        {
            if (sender == this.TextHex)
            {
                return;
            }
            if (Input == null || Input.Length == 0)
            {
                this.TextHex.Text = string.Empty;
            }
            else
            {
                this.TextHex.Text = BitConverter.ToString(Input).Replace("-", " ");
            }
        }

        private void UpdateFormattedHex(TextBox sender)
        {
            if (sender == this.TextFormattedHex)
            {
                return;
            }
            if (Input == null || Input.Length == 0)
            {
                this.TextFormattedHex.Text = string.Empty;
            }
            else
            {
                this.TextFormattedHex.Text = BitConverter.ToString(Input).Replace("-", " ");
            }
        }

        private void BtnMouseEnter(object sender, MouseEventArgs e)
        {
            object tag = ((Button)sender).Tag;
            if (tag != null)
            {
                this.TextToCopy.Text = Input == null || Input.Length == 0 ? string.Empty : Encoding.GetEncoding(int.Parse(tag.ToString())).GetString(Input);
            }
            else
            {
                switch (((Button)sender).Name)
                {
                    case "BtnCopyHex":
                        this.TextToCopy.Text = this.TextFormattedHex.Text.Replace(" ", string.Empty);
                        break;
                    case "BtnCopyHexWithSpace":
                        this.TextToCopy.Text = this.TextFormattedHex.Text;
                        break;
                    case "BtnCopyHexWithHyphen":
                        this.TextToCopy.Text = this.TextFormattedHex.Text.Replace(" ", "-");
                        break;
                    case "BtnCopyBase64":
                        this.TextToCopy.Text = Input == null ? string.Empty : Convert.ToBase64String(Input);
                        break;
                    default:
                        MessageBox.Show("Unknown button", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }

        private void BtnCopyClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.TextToCopy.Text))
            {
                return;
            }
            Clipboard.SetDataObject(this.TextToCopy.Text);
        }

        private void BtnClearClick(object sender, RoutedEventArgs e)
        {
            this.TextASCII.Clear();
            this.TextToCopy.Clear();
        }

        private void TextHexPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9A-Fa-f]$");
        }

        public byte[] HexStringToByteArray(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
            {
                return null;
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        private void AboutClick(object sender, RoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Owner = this;
            aboutBox.ShowDialog();
        }

        private void AddCustomEncodings()
        {
            if (CustomEncodings.Items == null || CustomEncodings.Items.Count == 0)
            {
                return;
            }
            foreach (Encoding encoding in CustomEncodings.Items)
            {
                if (encoding == null)
                {
                    continue;
                }
                GridString.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                TextBlock textBlock = new TextBlock
                {
                    Text = encoding.EncodingName,
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Foreground = Brushes.LightGray,
                };
                TextBox textBox = new TextBox
                {
                    Tag = encoding.CodePage,
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = (Brush)new BrushConverter().ConvertFrom("#ff313131"),
                    Foreground = Brushes.LightGray,
                };
                textBox.TextChanged += TextChanged;

                Grid.SetRow(textBlock, GridString.RowDefinitions.Count - 1);
                Grid.SetColumn(textBlock, 0);
                Grid.SetRow(textBox, GridString.RowDefinitions.Count - 1);
                Grid.SetColumn(textBox, 1);
                GridString.Children.Add(textBlock);
                GridString.Children.Add(textBox);

                Button button = new Button
                {
                    Tag = encoding.CodePage,
                    Content = encoding.EncodingName,
                    Margin = new Thickness(5, 10, 5, 10),
                };
                button.Click += BtnCopyClick;
                button.MouseEnter += BtnMouseEnter;
                WrapCopy.Children.Insert(WrapCopy.Children.Count - 4, button);
            }
        }
    }
}
