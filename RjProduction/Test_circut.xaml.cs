using RjProduction.Model;
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
using HtmlAgilityPack;
using System.IO;

namespace RjProduction
{
    public partial class Test_circut : Window
    {
      

        public Test_circut()
        {
            InitializeComponent();
           
        }

        private void ТестА(object sender, RoutedEventArgs e)
        {
           
           /* var y = Sql.SqlRequest.ReadData<DocRow>(1);
            y.Price = 777;
            Sql.SqlRequest.SetData(y);*/

           //t.Rollback();
        }

        private void ТестБ(object sender, RoutedEventArgs e)
        {
            string sfile = "C:\\Users\\Макс\\Desktop\\Новая папка\\test.xml";
            Fgis.XML.forestUsageReport forest = new Fgis.XML.forestUsageReport();
            forest.SaveXml(sfile);

           

        }

        private void ТестC(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
