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
using System.Windows.Media;
using RjProduction.Model.Classifier;

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

        public void AddNotification(NotifMessage notif)
        {
            if (NotificationPanel.Visibility == Visibility.Collapsed) NotificationPanel.Visibility = Visibility.Visible;

            notif.Close += Bag_Close;
            NotificationPanel.Children.Add(notif.CreateInstance());
            _ = notif.Start();

        }

        private void Bag_Close(NotifMessage b)
        {
            for (int i = 0; i < NotificationPanel.Children.Count; i++)
            {
                if (NotificationPanel.Children[i] is Button bi)
                {
                    if (((NotifMessage)bi.Tag) == b)
                    {
                        NotificationPanel.Children.RemoveAt(i);
                        break;
                    }
                }
            }
            if (NotificationPanel.Children.Count == 0) NotificationPanel.Visibility = Visibility.Collapsed;
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

            foreach (Control item in StackP_Buttons.Children) item.Background.Opacity = 0;
        }


        private void SelectButton(Button button) {
            
            foreach (var item in Sp_leftPanel.Children)
            {
                if (item is  Button  b) {
                    b.Background = null;
                } 
            }

            button.Background = MDL.BrushConv("#FFB6D4D1");
        }

        private void Загрузка(object sender, RoutedEventArgs e)
        {
            MDL.MainWindow = this;
#if DEBUG
            System.Diagnostics.Debug.WriteLine("<< Start programm Debug >>");
#endif

            LoadValues();   //Загрузка справочников и настроек

            FrameDisplay.Navigate(new Pages.PageStartPage()); // Стартовая страница

            string sFile = AppDomain.CurrentDomain.BaseDirectory + "xmldocs\\"; //Создание каталогов под файлы документов

            try
            {
                if (!File.Exists(sFile)) Directory.CreateDirectory(sFile); // Создание директории            
                if (Directory.Exists(AppContext.BaseDirectory + "Data") == false) Directory.CreateDirectory(AppContext.BaseDirectory + "Data");
                if (Directory.Exists(AppContext.BaseDirectory + "Update") == false) Directory.CreateDirectory(AppContext.BaseDirectory + "Update");
            }
            catch
            {
                MessageBox.Show("У программы нет прав на создание каталогов и файлов в рабочй папке. Это приложение должно быть установлено установщиком программы или изменить права доступа в данной папке.");
                ПриложениеЗакрыто(null!, null!);
            }

            ScreenRegulator(); // Управление основным окном (размеры и положение)          

            MDL.Refreh_AllWpfView(); // Обновить нижнаяя панель

            var test = new Test_circut();
            // test.ShowDialog();
           // var ls = CountryOKSM.LoadList(@"C:\Users\Макс\source\repos\RjProduction\RjProduction\Resources\oksm.xml");
           
        }

        static private void LoadValues() { 
         XmlSerializer xmlSerializer;
            if (File.Exists(SFile_DB))
            {
                xmlSerializer = new(typeof(Reference));
                using FileStream fs = new(SFile_DB, FileMode.OpenOrCreate);
                MDL.MyDataBase = xmlSerializer.Deserialize(fs) as Reference ?? new Reference();
            }
            if (File.Exists(MDL.SetApp.SetFile))
            {
                xmlSerializer = new(typeof(MDL.SettingAppClass));
                using FileStream fss = new(MDL.SetApp.SetFile, FileMode.OpenOrCreate);
                MDL.SetApp = xmlSerializer.Deserialize(fss) as SettingAppClass ?? new SettingAppClass();
                MDL.SetApp.SetProfile();
            }
        }
                
        private void ScreenRegulator()
        {
            if (MDL.WindowsStandart)
            {
                UpPanel.Visibility = Visibility.Collapsed;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }
            else
            {
                UpPanel.Visibility = Visibility.Visible;
                this.WindowStyle = WindowStyle.None;
            }
           

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

            if (this.Top < 0) this.Top = 0;
            if (this.Left < 0) this.Left = 0;
             WindowState = MDL.SetApp.WindowStateDef;
        }

        private void ПриложениеЗакрыто(object sender, EventArgs e)
        {
            MDL.Reference.SaveDB();
            MDL.SaveSettingApp();
            MDL.SqlProfile?.Dispose();
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

            //Grid_Task.Visibility = Visibility.Visible;
            FrameDisplay.Navigate(new Pages.MainPage.PageFgis());

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
            SelectButton((Button)sender);
            string filePath = AppDomain.CurrentDomain.BaseDirectory + $"xmldocs\\{DateTime.Now.Year}\\{DateTime.Now.Month}\\";
            if (!Directory.Exists(filePath)) return;
            try
            {
                Process.Start("explorer.exe", "/select, \"" + filePath + "\"");
            }
            catch 
            {
                Grid_AntiVirus.Visibility = Visibility.Visible;
            }            
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
            MDL.SetApp.WindowStateDef = this.WindowState;
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
            FrameDisplay.Navigate(new PageSetApp());
        }

        private void ДокументыБД(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            FrameDisplay.Navigate(new Pages.Additions.PageDocs());
        }

        private void СмотретьОстаткиСклада(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            // FrameDisplay.Navigate(new Pages.PageRemains());
            FrameDisplay.Navigate(new Pages.Additions.PageRemProduct());
        }

        private void ПереходПроизводство2(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            FrameDisplay.Navigate(new Pages.PageArrival());
        }

        private void ОткрытьСклады(object sender, RoutedEventArgs e)
        {
            if (Reference is PageReference r) r.МоиСклады();

        }

        private void Калькулятор(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process p = System.Diagnostics.Process.Start("calc.exe");
            p.WaitForInputIdle();
           
        }

        private void НастройкиАкка(object sender, KeyEventArgs e)
        {
            var wpf = new MyAcc();
            wpf.Show();
        }

        private void ГорячиеКлавиши(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12) {
                MDL.AddNotification("test");
            
            }
        }

        private void КурсорНадАвой(object sender, MouseEventArgs e)=> ElipsAva.Stroke = Brushes.GreenYellow;
        private void КурсорПокинулАву(object sender, MouseEventArgs e)=> ElipsAva.Stroke = Brushes.White;
        private void КурсорСуппорт(object sender, MouseEventArgs e)=> Support.Foreground = Brushes.GreenYellow;
        private void КурсорВышелСуппорт(object sender, MouseEventArgs e) => Support.Foreground = Brushes.White;

        private void НажатьАккаунт(object sender, MouseButtonEventArgs e)
        {
            var wpf = new WpfFrm.MyAcc();
            wpf.Show();
        }

        private void ОткрытьАдреса(object sender, RoutedEventArgs e)
        {
            if (Reference is PageReference r) r.МоиАдреса();
        }

        private void ВыборОтчетПоступление(object sender, MouseButtonEventArgs e)
        {
            if (ReportPage != null)
            {
                ReportPage.Button_GeneratedReport.Visibility = Visibility.Visible;
                ReportPage.Button_SetUpReport.Visibility = Visibility.Visible;
                ReportPage.Grid_DateSelect.Visibility = Visibility.Visible;
                ReportPage.SelectReport = ReportPage.Report_Gen_ВсеДниРабочих;
                ReportPage.Grid_Start.Visibility = Visibility.Collapsed;
                ReportPage.Label_Title.Content = "Продукция доставленная автотранспортом";
            }
        }
    }
}