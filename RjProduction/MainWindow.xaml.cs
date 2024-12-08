using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using static RjProduction.MDL;
using System.Xml.Serialization;
using RjProduction.WpfFrm;
using System.Diagnostics;
using RjProduction.Pages;
using RjProduction.Model;

namespace RjProduction
{
    public partial class MainWindow : Window
    {
        private PageReport? ReportPage;
        private PageReference? Reference;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Возврат в первоначальное значение визуальных элементов
        /// </summary>
        private void ClearButtonSel()
        {
            FrameDisplay.NavigationService.RemoveBackEntry();
            Grid_Task.Visibility = Visibility.Collapsed;
            Grid_Reports.Visibility = Visibility.Collapsed;
            Grid_Arrival.Visibility = Visibility.Collapsed;
            Grid_Reference.Visibility = Visibility.Collapsed;

            foreach (Control item in StackP_Buttons.Children)
            {
                item.Background.Opacity = 0;
            }
        }


        private void Загрузка(object sender, RoutedEventArgs e)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("<< Start programm Debug >>");
#endif
            if (!File.Exists(SFile_DB)) return;
            XmlSerializer xmlSerializer = new(typeof(BoardDic));
            using FileStream fs = new(SFile_DB, FileMode.OpenOrCreate);
            MDL.MyDataBase = xmlSerializer.Deserialize(fs) as BoardDic ?? new BoardDic();

            FrameDisplay.Navigate(new Pages.PageStartPage());

            //Создание каталогов под файлы документов
            string sFile = AppDomain.CurrentDomain.BaseDirectory + "xmldocs\\";

            if (!File.Exists(sFile)) Directory.CreateDirectory(sFile);
            WindowState =MDL.MyDataBase.WindowStateDef;

            if (Directory.Exists(AppContext.BaseDirectory + "Data") == false) Directory.CreateDirectory(AppContext.BaseDirectory + "Data");




            var s = new Sql.SqliteProfile() { DataBase = "test" };




         //  Sql.SqlRequest.SetData<Model.WarehouseClass>(s, new WarehouseClass() {  DescriptionWarehouse ="0", NameWarehouse ="name"  });

          // Sql.SqlRequest.SetData(s, new Products() {  Cubature =0, NameItem ="test", Quantity =5, Warehouse = new WarehouseClass() { NameWarehouse ="NameTest" } } );


           //  var tttt=  Sql.SqlRequest.ReadData <Model.Products>( s,1);
           //  Sql.SqlRequest.CreateTabel<Model.WarehouseClass>( s);
           //  Sql.SqlRequest.CreateTabel<Model.Products>( s );
        }

        private void ПриложениеЗакрыто(object sender, EventArgs e)
        {
            MDL.SaveXml<MDL.BoardDic>(MDL.MyDataBase, MDL.SFile_DB);
            Application.Current.Shutdown();
        }

        private void ПереходПроизводство(object sender, RoutedEventArgs e)
        {
            ClearButtonSel();
            ((Button)sender).Background.Opacity = 1;
            Grid_Arrival.Visibility = Visibility.Visible;
            FrameDisplay.Navigate(new Pages.PageArrival());
        }

        private void СправочникОткрыть(object sender, RoutedEventArgs e)
        {
            ClearButtonSel();
            ((Button)sender).Background.Opacity = 1;

            Reference = new();
            FrameDisplay.Navigate(Reference);
            Grid_Reference.Visibility = Visibility.Visible;
        }

        private void ОткрытьОтчеты(object sender, RoutedEventArgs e)
        {
            ClearButtonSel();
            ((Button)sender).Background.Opacity = 1;
            Grid_Reports.Visibility = Visibility.Visible;
            ReportPage = new PageReport();
            FrameDisplay.Navigate(ReportPage);
        }

        private void ФинансыОткрыть(object sender, RoutedEventArgs e)
        {
            ClearButtonSel();
            ((Button)sender).Background.Opacity = 1;
            FrameDisplay.Navigate(new Pages.Page1());
        }

        private void ОткрытьЗадачи(object sender, RoutedEventArgs e)
        {
            ClearButtonSel();
            ((Button)sender).Background.Opacity = 1;

            Grid_Task.Visibility = Visibility.Visible;
            FrameDisplay.Navigate(new Pages.Page1());

        }

        private void ВыборОтчет1(object sender, MouseButtonEventArgs e)
        {
            if (ReportPage != null)
            {
                ReportPage.Button_GeneratedReport.Visibility = Visibility.Visible;
                ReportPage.Button_SetUpReport.Visibility = Visibility.Visible;
                ReportPage.Grid_DateSelect.Visibility = Visibility.Visible;
                ReportPage.SelectReport = ReportPage.Report_Gen_МесячнаяЗарплата;
                ReportPage.Grid_Start.Visibility = Visibility.Collapsed;
                ReportPage.Label_Title.Content = "Отчет по зарплатам";
            }
        }

        private void СтартоваяСтраница(object sender, MouseButtonEventArgs e)
        {
            ClearButtonSel();
            FrameDisplay.Navigate(new Pages.PageStartPage());
        }

        private void ВыборОтчет2(object sender, MouseButtonEventArgs e)
        {
            if (ReportPage != null)
            {
                ReportPage.Button_GeneratedReport.Visibility = Visibility.Visible;
                ReportPage.Button_SetUpReport.Visibility = Visibility.Visible;
                ReportPage.Grid_DateSelect.Visibility = Visibility.Visible;
                ReportPage.SelectReport = ReportPage.Report_Gen_ВсеДниРабочих;
                ReportPage.Grid_Start.Visibility = Visibility.Collapsed;
                ReportPage.Label_Title.Content = "Подробный отчет по всем сотрудникам";
            }
        }

        private void ВыборЗадачи(object sender, MouseButtonEventArgs e)
        {
            var wpf = new WpfInfoTask();
            wpf.ShowDialog();
        }

        private void ОткрытьПапку(object sender, RoutedEventArgs e)
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + $"xmldocs\\{DateTime.Now.Year}\\{DateTime.Now.Month}\\";
            if (!Directory.Exists(filePath)) return;
            Parallel.Invoke(() => Grid_AntiVirus.Visibility = Visibility.Visible);
            Process.Start("explorer.exe", "/select, \"" + filePath + "\"");
        }

        private void Перетаскивание(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2) { КружокРазвернутьПрограмму(null!, null!); }
            else if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void КурсорВыбралЭлемент(object sender, MouseEventArgs e) => ((Grid)sender).Background = MDL.BrushConv("#FFD7D7D7");
        private void ВыходИзФокуса(object sender, MouseEventArgs e) => ((Grid)sender).Background = Grid_Reports.Background;
        private void КурсорНадКружком(object sender, MouseEventArgs e) => ((Ellipse)sender).Fill.Opacity = 1;
        private void КурсорСошелКружка(object sender, MouseEventArgs e) => ((Ellipse)sender).Fill.Opacity = 0.5;
        private void КружокЗакрытьПрограмму(object sender, MouseButtonEventArgs e) => Application.Current.Shutdown();
        private void КружокТрейПрограмму(object sender, MouseButtonEventArgs e) => WindowState = WindowState.Minimized;
        private void СправочникКнопкаВыдел(object sender, MouseEventArgs e) => ((Button)sender).Background.Opacity = 1;
        private void СправочникКнопкаВыход(object sender, MouseEventArgs e) => ((Button)sender).Background.Opacity = 0;

        private void КурсорНадИконКнопкой(object sender, MouseEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Background.Opacity != 1) button.Background.Opacity = 0.3;
        }
        private void КурсорПокинулИконКнопкой(object sender, MouseEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Background.Opacity != 1) button.Background.Opacity = 0;
        }

        private void КружокРазвернутьПрограмму(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;
            MDL.MyDataBase.WindowStateDef = this.WindowState;
        }

        private void ОткрытьСотрудники(object sender, RoutedEventArgs e)
        {
            if (Reference is PageReference r) r.МоиСотрудники(null!, null!);
        }

        private void ОткрытьМатериалы(object sender, RoutedEventArgs e)
        {
            if (Reference is PageReference r) r.МоиМатериалы(null!, null!);
        }

        private void ОткрытьГруппы(object sender, RoutedEventArgs e)
        {
            if (Reference is PageReference r) r.МоиГруппы(null!, null!);
        }

        private void ОткрытьНастройки(object sender, RoutedEventArgs e)
        {
            ClearButtonSel();
            ((Button)sender).Background.Opacity = 1;
            FrameDisplay.Navigate(new Page1());
        }
    }
}