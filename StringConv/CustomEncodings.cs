using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace StringConv
{
    public sealed class CustomEncodings
    {
        private static readonly string CustomEncodingFileName = "CustomEncodings.txt";

        public static List<Encoding> Items => TryGetEncodings();

        private static List<Encoding> TryGetEncodings()
        {
            if (!File.Exists(CustomEncodingFileName))
            {
                CreateEmptyFile();
                return null;
            }
            List<Encoding> l = new List<Encoding>();
            try
            {
                foreach (var line in File.ReadLines(CustomEncodingFileName))
                {
                    if (int.TryParse(line, out int codePage))
                    {
                        var encoding = Encoding.GetEncoding(codePage);
                        if (encoding != null)
                        {
                            l.Add(encoding);
                        }
                    }
                }
                return l;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading custom encodings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                l.Clear();
                return null;
            }
        }

        private static void CreateEmptyFile()
        {
            using (var fs = File.Create(CustomEncodingFileName))
            {
            }
        }
    }
}
