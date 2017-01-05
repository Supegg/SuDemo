using Microsoft.Win32;
using SuUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace SuCreaetor.Creator
{
    /// <summary>
    /// UClte.xaml 的交互逻辑
    /// </summary>
    public partial class UClte : UserControl
    {
        public UClte()
        {
            InitializeComponent();
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Lte Order(*.xml)|*.xml";// "Lte Order(*.xml)|*.xml|All Files(*.*)|*.*";
            string file = string.Empty;
            if (dialog.ShowDialog() == true)
            {
                file = dialog.FileName;
                txtFile.Text = file;
            }
            else
            {
                return;
            }

            using (FileStream reader = File.OpenRead(file))
            {
                byte[] buffer = new byte[reader.Length];
                reader.Read(buffer, 0, buffer.Length);
                byte[] garbled = SuEncipher.Encipher(buffer);

                string sulte = file.Substring(0, file.LastIndexOf('.')) + ".sulte";
                using (FileStream writer = new FileStream(sulte, FileMode.Create))
                {
                    writer.Write(garbled, 0, garbled.Length);
                    writer.Flush();
                }
            }

            //Decipher demo
            using (FileStream reader = File.OpenRead(file.Substring(0, file.LastIndexOf('.')) + ".sulte"))
            {
                byte[] buffer = new byte[reader.Length];
                reader.Read(buffer, 0, buffer.Length);
                byte[] raw = SuEncipher.Decipher(buffer);
                string xml = Encoding.UTF8.GetString(raw);
            }

            MessageBox.Show("转换成功", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
