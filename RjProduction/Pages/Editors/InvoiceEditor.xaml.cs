using RjProduction.Model.Classifier;
using RjProduction.XML.BlankDoc;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RjProduction.Pages.Editors
{  
    public partial class InvoiceEditor : Page
    {
        private Invoice _Invoice;
        private readonly List<UnitMeasurement> List_UnitMeasurement = UnitMeasurement.LoadList(MDL.Dir_Resources + "okei.xml");
        
        public InvoiceEditor(Invoice invoice )
        {
            InitializeComponent();
            _Invoice=invoice;
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            UnitMeasurement def = List_UnitMeasurement.Find(x => x.Code == 113); // Кубический метр
            ushort i = 1;
            foreach (var item in _Invoice.Tabel)
            {
                if (item.Packaging.UnitMeas.IsDefault) item.Packaging.UnitMeas = def;
                item.Num = i;
                i++;
            }
        }

        private void СохранитьXML(object sender, RoutedEventArgs e)
        {

        }

        private void СоздатьСтроку(object sender, RoutedEventArgs e)
        {

        }

        private void РазвернутьЭкран(object sender, RoutedEventArgs e)
        {

        }

        private void НажитиеЗакрыть(object sender, RoutedEventArgs e)
        {

        }

        private void ВыходИзДаты_(object sender, RoutedEventArgs e)
        {

        }

        private void ВходВПоле_(object sender, RoutedEventArgs e)
        {

        }

        private void ВодНомера(object sender, RoutedEventArgs e)
        {

        }



        

    }
}
