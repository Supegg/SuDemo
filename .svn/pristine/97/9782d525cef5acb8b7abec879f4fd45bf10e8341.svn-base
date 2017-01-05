using System;
using System.Collections.Generic;
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
using SuUtil;
using Microsoft.Win32;
using System.IO;

namespace SuCreaetor.Creator
{
    /// <summary>
    /// UCbds.xaml 的交互逻辑
    /// </summary>
    public partial class UCbds : UserControl
    {
        public UCbds()
        {
            InitializeComponent();
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "BDS Order(*.xml)|*.xml";// "BDS Order(*.xml)|*.xml|All Files(*.*)|*.*";
            string file = string.Empty;
            if(dialog.ShowDialog()==true)
            {
                file = dialog.FileName;
                txtFile.Text = file;
            }else
            {
                return;
            }

            using(FileStream reader = File.OpenRead(file))
            {
                byte[] buffer = new byte[reader.Length];
                reader.Read(buffer, 0, buffer.Length);
                byte[] garbled = SuEncipher.Encipher(buffer);

                string subds = file.Substring(0, file.LastIndexOf('.')) + ".subds";
                using(FileStream writer = new FileStream(subds,FileMode.Create))
                {
                    writer.Write(garbled, 0, garbled.Length);
                }
            }

            //Decipher demo
            using (FileStream reader = File.OpenRead(file.Substring(0, file.LastIndexOf('.')) + ".subds"))
            {
                byte[] buffer = new byte[reader.Length];
                reader.Read(buffer, 0, buffer.Length);
                byte[] raw = SuEncipher.Decipher(buffer);
                string xml = Encoding.UTF8.GetString(raw);
            }

            MessageBox.Show("转换成功","OK",MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
