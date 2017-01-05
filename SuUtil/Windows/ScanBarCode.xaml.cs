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
using System.Windows.Shapes;
using System.Xml.Linq;

namespace SuUtil.Windows
{
    #region using
    //ScanBarCode scan = new ScanBarCode();
    //scan.Owner = this;
    //scan.ShowDialog();
    //code = scan.BarCode;
    //scan.Close();
    #endregion

    /// <summary>
    /// ScanBarCode.xaml 的交互逻辑
    /// </summary>
    public partial class ScanBarCode : Window
    {
        private string barCode = string.Empty;
        public string BarCode { get { return barCode; } }

        public ScanBarCode()
        {
            InitializeComponent();

            this.Loaded += ScanBarCode_Loaded;
        }

        private void ScanBarCode_Loaded(object sender, RoutedEventArgs e)
        {

            this.txtBarCode.Focus();
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            barCode = this.txtBarCode.Text.Trim();
            this.Hide();
        }
    }
}
