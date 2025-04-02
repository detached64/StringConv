using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StringConv
{
    public partial class MainWindow : Window
    {
        private bool IsUpdating = false;
        private Encoding SelectedEncoding => this.CombCustomEncoding.SelectedItem as Encoding;
        public byte[] Input = null;
        private StringToCopy TextCopy = new StringToCopy();

        public MainWindow()
        {
            InitializeComponent();
            this.CombCustomEncoding.ItemsSource = Items;
            this.CombCustomEncoding.SelectedIndex = Settings.Default.SelectedEncodingIndex;
        }

        private List<Encoding> Items => new List<Encoding>()
        {
            Encoding.UTF8,
            Encoding.GetEncoding(932),
            Encoding.GetEncoding(936),
        };

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
            UpdateAscii(textBox);
            UpdateUnicode(textBox);
            UpdateCustom(textBox);
            UpdateHex(textBox);
            UpdateFormattedHex(textBox);
            GenerateCopyString();
            UpdateByteCount();
            IsUpdating = false;
        }

        private byte[] ProcessString(string input, TextBox sender)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            switch (sender.Name)
            {
                case "TextASCII":
                    return Encoding.ASCII.GetBytes(input);
                case "TextUnicode":
                    return Encoding.Unicode.GetBytes(input);
                case "TextCustomEncoding":
                    return SelectedEncoding.GetBytes(input);
                case "TextHex":
                    return HexStringToByteArray(new string(input.Where(c => Uri.IsHexDigit(c)).ToArray()).ToUpper());
                default:
                    throw new Exception("Unknown sender");
            }
        }

        private void GenerateCopyString()
        {
            TextCopy = new StringToCopy
            {
                TextASCII = this.TextASCII.Text,
                TextUnicode = this.TextUnicode.Text,
                TextCustomEncoding = this.TextCustomEncoding.Text,
                TextHex = this.TextFormattedHex.Text.Replace(" ", string.Empty),
                TextHexWithSpace = this.TextFormattedHex.Text,
                TextHexWithHyphen = this.TextFormattedHex.Text.Replace(" ", "-"),
                TextBase64 = Input == null ? string.Empty : Convert.ToBase64String(Input)
            };
        }

        private void UpdateByteCount()
        {
            this.TextByteCount.Text = Input == null ? "0" : Input.Length.ToString();
        }

        private void UpdateAscii(TextBox sender)
        {
            if (sender == this.TextASCII)
            {
                return;
            }
            if (Input == null || Input.Length == 0)
            {
                this.TextASCII.Text = string.Empty;
            }
            else
            {
                this.TextASCII.Text = Encoding.ASCII.GetString(Input);
            }
        }

        private void UpdateUnicode(TextBox sender)
        {
            if (sender == this.TextUnicode)
            {
                return;
            }
            if (Input == null || Input.Length == 0)
            {
                this.TextUnicode.Text = string.Empty;
            }
            else
            {
                this.TextUnicode.Text = Encoding.Unicode.GetString(Input);
            }
        }

        private void UpdateCustom(TextBox sender)
        {
            if (sender == this.TextCustomEncoding)
            {
                return;
            }
            if (Input == null || Input.Length == 0)
            {
                this.TextCustomEncoding.Text = string.Empty;
            }
            else
            {
                this.TextCustomEncoding.Text = SelectedEncoding.GetString(Input);
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
            switch (((Button)sender).Name)
            {
                case "BtnCopyASCII":
                    this.TextToCopy.Text = TextCopy.TextASCII;
                    break;
                case "BtnCopyUnicode":
                    this.TextToCopy.Text = TextCopy.TextUnicode;
                    break;
                case "BtnCopyCustomEncoding":
                    this.TextToCopy.Text = TextCopy.TextCustomEncoding;
                    break;
                case "BtnCopyHex":
                    this.TextToCopy.Text = TextCopy.TextHex;
                    break;
                case "BtnCopyHexWithSpace":
                    this.TextToCopy.Text = TextCopy.TextHexWithSpace;
                    break;
                case "BtnCopyHexWithHyphen":
                    this.TextToCopy.Text = TextCopy.TextHexWithHyphen;
                    break;
                case "BtnCopyBase64":
                    this.TextToCopy.Text = TextCopy.TextBase64;
                    break;
                default:
                    throw new Exception("Unknown sender");
            }
        }

        private void BtnMouseLeave(object sender, MouseEventArgs e)
        {
            //this.TextToCopy.Clear();
        }

        private void BtnCopyClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.TextToCopy.Text))
            {
                return;
            }
            Clipboard.SetDataObject(this.TextToCopy.Text);
        }

        private void CombCustomEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.SelectedEncodingIndex = this.CombCustomEncoding.SelectedIndex;
            Settings.Default.Save();
            this.BtnCopyCustomEncoding.Content = SelectedEncoding.WebName.ToUpper();
            TextChanged(this.TextCustomEncoding, null);
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            this.TextASCII.Clear();
            this.TextToCopy.Clear();
        }

        private void TextHex_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9A-Fa-f]$");
        }

        private void TextHex_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
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

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Owner = this;
            aboutBox.ShowDialog();
        }
    }
}
