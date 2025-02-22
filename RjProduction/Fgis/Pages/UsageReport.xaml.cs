using RjProduction.Fgis.WPF;
using RjProduction.Fgis.XML;
using RjProduction.Model;
using RjProduction.Pages.MainPage;
using RjProduction.WpfFrm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
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
using static RjProduction.Fgis.XML.forestUsageReport;

namespace RjProduction.Fgis.Pages
{
    
    public partial class UsageReport : Page
    {
        private KindTypes _KindTypes;
        public forestUsageReport _Document;


        public UsageReport(forestUsageReport document, PageFgis.Config_fgis config )
        {
            InitializeComponent();
            _KindTypes = new KindTypes(config);
            _Document = document;
        }

        /// <summary>
        /// Получить данные из Page
        /// </summary>
        private void DataExtractor(EmployeeType  cl, Grid grid ,out bool error ) {
            error = true;
            foreach (var item in cl.GetType().GetProperties())
            {
                Mod? mod = Extract_Mod(item);
                if (mod is null) continue;
                UIElement? ui = find_ui(item.Name, grid);
                if (ui is not null)
                {
                    string str = "";
                    if (ui is TextBox text) str=text.Text;
                    else if (ui is DatePicker picker)
                    {
                        if (picker.SelectedDate is null) str = "";
                        else str = picker.SelectedDate.Value.ToString("yyyy-MM-dd");
                    }
                    else if (ui is Button button)
                    {
                        // Добавить класс

                    }
                    if (Mod.CheckRule(str, mod) == false)
                    {
                        error = false;
                        ((Control)ui).Background = Brushes.MistyRose;
                    }
                    else
                    {
                        ((Control)ui).Background = Brushes.White;
                        item.SetValue(cl, str);
                    }
                }
            }

        }

        UIElement? find_ui(string name, Grid grid)
        {
            foreach (UIElement tv in grid.Children)
            {
                if (tv.Uid == name)
                {
                    return tv;
                }
            }
            return null;
        }
        private Mod? Extract_Mod(PropertyInfo property)
        {
            foreach (var m in property.GetCustomAttributes(false))
            {
                if (m is Mod)
                {
                    return (Mod)m;
                }
            }
            return null;
        }

      
        private void Загруженно(object sender, RoutedEventArgs e)
        {
            Grind_Tabel.Visibility = Visibility.Collapsed;
            TextBox_Number.Focus();
            DateNow.SelectedDate = DateTime.Now;
            CBox_Countres.ItemsSource = _KindTypes.GetDate(KindTypes.TypesEnum.dConstituentEntity);
        }

        private void ВыбратьОрганизацию(object sender, RoutedEventArgs e)
        {
            var r = new Fgis.WPF.DicWindows(KindTypes.TypesEnum.dSubordinateAuthority, (dl) =>
            {
                Label_MSU.Text = dl.Name;
                Label_MSU.Tag = dl;
                Button_MSU.FontWeight = FontWeights.Normal;
            }, _KindTypes);
            r.ShowDialog();
        }

        private void ВыбратьДоговор(object sender, RoutedEventArgs e)
        {
            var r = new Fgis.WPF.DicWindows( WPF.DicWindows.TypeElementFgis.Contract, (dl) =>
            {
                Button_Contract.Content  = dl.Name;
                Button_Contract.Tag = dl;
            },_KindTypes);
            r.ShowDialog();
        }

        private void Создать(object sender, RoutedEventArgs e)
        {
            var test = new DialogWindow(new forestUsageReport.TreesInfoTypeList(), (cl) => { }, _KindTypes);
            test.ShowDialog();
        }

        private void Изменить(object sender, RoutedEventArgs e)
        {

        }

        private void Удалить(object sender, RoutedEventArgs e)
        {

        }

        private void Дублировать(object sender, RoutedEventArgs e)
        {

        }

        private void ОтправительДокументов(object sender, RoutedEventArgs e)
        {
            var r = new Fgis.WPF.DicWindows(WPF.DicWindows.TypeElementFgis.Company, (dl) =>
            {
                Button_Company.Content = dl.Name;
                Button_Company.Tag = dl;
            }, _KindTypes);
            r.ShowDialog();

           
        }

        private void ВыборЗначения(object sender, RoutedEventArgs e)
        {
            Label_Title.Content = ((RadioButton)sender).Content;
            Grind_Tabel.Visibility = Visibility.Visible;
        }

        private void ВыбратьСотрудника(object sender, RoutedEventArgs e)
        {
            //DataExtractor(new EmployeeType(), Cl_EmployeeType , out bool error);
            var r = new Fgis.WPF.DicWindows( DicWindows.TypeElementFgis.Employee, (dl) =>
            {
                Button_Empl.Content = dl.Name;
                Button_Empl.Tag = dl.Obj;
            }, _KindTypes);
            r.ShowDialog();
        }

        private void ВыбранаСтрока_(object sender, SelectionChangedEventArgs e)
        {
            CBox_Forest.IsEnabled = true;
            if (CBox_Countres.SelectedItem is DeliveredStruct delivered)
            {
                string f = delivered.Comment + ":";
                CBox_Forest.ItemsSource = from tv in _KindTypes.GetDate(KindTypes.TypesEnum.dForestry) where tv.Comment.IndexOf(f) != -1 select tv;
            }
        }

        private void Refreh_Attachmen()
        {
            if (_Document.attachments is null) return;
            ListBox_FileList.Items.Clear();
            foreach (var item in _Document.attachments.attachmen!)
            {
                string s;
                if (item.desc == string.Empty) s = item.file.fileURI;
                else s = item.desc;
                ListBox_FileList.Items.Add(s);
            }
        }

        private void ДобавитьФайл(object sender, RoutedEventArgs e)
        {
            _Document.attachments ??= new();
            _Document.attachments.attachmen ??= [];
            var win = new DialogAttachment(new(), (atach) =>
            {
                if (_Document.attachments.attachmen.Any(x => x.file.fileURI == atach.file.fileURI)) {
                    MessageBox.Show("Ранее этот файл был уже добавлен в список");
                    return;
                }
                _Document.attachments.attachmen.Add(atach);
                Refreh_Attachmen();
            });
            win.ShowDialog();
        }

        private void СоздатьФайл(object sender, RoutedEventArgs e)
        {
            // Копирование файлов
            if (_Document.attachments is not null )  { 
                        

            }


        }
    }
}
