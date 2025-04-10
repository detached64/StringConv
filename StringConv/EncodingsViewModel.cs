using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Windows;

namespace StringConv
{
    public sealed class EncodingsViewModel : INotifyCollectionChanged
    {
        private static readonly Lazy<EncodingsViewModel> lazyInstance = new Lazy<EncodingsViewModel>(() => new EncodingsViewModel());
        public static EncodingsViewModel Instance => lazyInstance.Value;

        private ObservableCollection<Encoding> _encodings;
        public ObservableCollection<Encoding> Encodings
        {
            get
            {
                if (_encodings == null)
                {
                    if (!File.Exists(CustomEncodingFileName))
                    {
                        _encodings = new ObservableCollection<Encoding>();
                    }
                    else
                    {
                        _encodings = ReadEncodings();
                    }
                }
                return _encodings;
            }
            set
            {
                if (_encodings != value)
                {
                    _encodings = value;
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }

        public void AddAll(List<Encoding> encodings)
        {
            if (encodings != null)
            {
                foreach (Encoding encoding in encodings)
                {
                    if (!Encodings.Contains(encoding))
                    {
                        Encodings.Add(encoding);
                    }
                }
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void RemoveAll(List<Encoding> encodings)
        {
            if (encodings != null)
            {
                foreach (Encoding encoding in encodings)
                {
                    Encodings.Remove(encoding);
                }
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void Clear()
        {
            Encodings.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            WriteEncodings();
            CollectionChanged?.Invoke(this, e);
        }

        private const string CustomEncodingFileName = "CustomEncodings.txt";

        private ObservableCollection<Encoding> ReadEncodings()
        {
            ObservableCollection<Encoding> encodings = new ObservableCollection<Encoding>();
            try
            {
                foreach (var line in File.ReadLines(CustomEncodingFileName))
                {
                    if (int.TryParse(line, out int codePage))
                    {
                        var encoding = Encoding.GetEncoding(codePage);
                        if (encoding != null)
                        {
                            encodings.Add(encoding);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading custom encodings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return encodings;
        }

        private void WriteEncodings()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(CustomEncodingFileName))
                {
                    foreach (var encoding in Encodings)
                    {
                        writer.WriteLine(encoding.CodePage);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing custom encodings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
