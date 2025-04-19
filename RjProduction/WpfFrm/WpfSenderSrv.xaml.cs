using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RjProduction.WpfFrm
{
    /// <summary>
    /// Логика взаимодействия для WpfSenderSrv.xaml
    /// </summary>
    public partial class WpfSenderSrv : Window
    {
        private readonly string[] Text_Array;

        public WpfSenderSrv(string[] ls_txt)
        {
            InitializeComponent();
            Text_Array = ls_txt;
        }

        private void Закрыть(object sender, RoutedEventArgs e)
        {

        }

        private void Отправить(object sender, RoutedEventArgs e)
        {

        }
    }
}
