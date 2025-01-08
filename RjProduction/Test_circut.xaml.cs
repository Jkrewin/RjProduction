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

namespace RjProduction
{
    public partial class Test_circut : Window
    {
        Model.Products testCl;
       

        public Test_circut()
        {
            InitializeComponent();
        }

        private void ТестА(object sender, RoutedEventArgs e)
        {
            testCl =  Sql.SqlRequest.ReadData<Model.Products>(1,true);
           
        }

        private void ТестБ(object sender, RoutedEventArgs e)
        {
            Sql.SqlRequest.SetData(testCl);
        }

        private void ТестC(object sender, RoutedEventArgs e)
        {
            testCl.LockInfo = false;
        }
    }
}
