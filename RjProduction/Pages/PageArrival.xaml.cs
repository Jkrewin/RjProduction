using RjProduction.Model;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static RjProduction.Model.Document;

namespace RjProduction.Pages
{
    public partial class PageArrival : Page
    {
        private List<Document> Docs = [];

        public PageArrival()
        {
            InitializeComponent();
        }

        private void Загрузка(object sender, RoutedEventArgs e)
        {           
            Refreh_DataG_Main();
        }

        private void Refreh_DataG_Main()
        {
            Docs = MDL.GetDocuments() ?? [];
            DataG_Main.ItemsSource = Docs;
            for (int i = 5; i < DataG_Main.Columns.Count; i++) DataG_Main.Columns[i].Visibility = Visibility.Collapsed;
            DataG_Main.Items.Refresh();
        }

        private void ДвойноеНажатие(object sender, MouseButtonEventArgs e)
        {
            if (DataG_Main.SelectedItem == null) return;
            DockPanel_РамкаДокумента.Visibility = Visibility.Visible;          
            FrameDisplay.Navigate(new PageDocEditor((Document)DataG_Main.SelectedItem, () => { 
                DockPanel_РамкаДокумента.Visibility = Visibility.Collapsed;
                Refreh_DataG_Main();
            }));
        }


        private void ОткрытьОкноОбъектов(object sender, RoutedEventArgs e)
        {
            FrameDisplay.NavigationService.RemoveBackEntry();
            DockPanel_РамкаДокумента.Visibility = Visibility.Visible;
            var doc = new Document() { Number = MDL.MyDataBase.NumberDef };
            doc.MainTabel.Add(new GrupObj() { NameGrup = "Основная" });
            FrameDisplay.Navigate(new PageDocEditor(doc, () =>
            {
                DockPanel_РамкаДокумента.Visibility = Visibility.Collapsed;
                Refreh_DataG_Main();
            }));
        }

        private void ПровестиДок(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Документ невозможно провести сейчас.");
        }

        private void УдалитьОбъект(object sender, RoutedEventArgs e)
        {
            Document d = (Document)DataG_Main.SelectedItem;
            XML.XmlDocument xml = new() { DataCreate = d.DataCreate.ToString("dd.MM.yyyy"), Number = d.Number };
            string sFile = AppDomain.CurrentDomain.BaseDirectory + "xmldocs\\";
            sFile += d.DataCreate.Year.ToString() +"\\"+ d.DataCreate.Month.ToString() +"\\" + xml.FileName;
            
            if (File.Exists(sFile))
            {
                try
                {
                    File.Delete(sFile);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            Refreh_DataG_Main();
        }
    }



}
