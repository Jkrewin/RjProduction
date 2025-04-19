
using RjProduction.Sql;
using RjProduction.XML;
using System.Windows;
using System.Windows.Controls;

namespace RjProduction.Pages
{
    public partial class PageSetApp : Page
    {
        const string SELECT_FORM = "#FFF0F8FD";
        bool first_start = false;
        
        public PageSetApp()
        {
            InitializeComponent();
        }

        private void Создать_профиль(object sender, RoutedEventArgs e)
        {
            MDL.SetApp.DataBaseFile = Tbox_Filename.Text;
            MDL.SetApp.LocalDir = Label_local_base.Content.ToString() ?? string.Empty;

            try
            {
                MDL.SetApp.SetProfile();               
                Sql.SqlRequest.CreateStartBaseTabel();    
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
                MDL.SqlProfile = null;
                return;
            }

            MDL.SaveSettingApp();
            MessageBox.Show("Профиль подключен успешно");
        }

        private void Изменить_Путь(object sender, RoutedEventArgs e)
        {
            var t = new SqliteProfile();
            // Временный способ следует заменить его 
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                InitialDirectory = t.LocalDir,
                Title = "Выберете путь папке с БД",
                Filter = "Directory|*.эта.папка",
                FileName = "выбрана" 
            };
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                path = path.Replace("\\выбрана.эта.папка", "");
                path = path.Replace(".эта.папка", "");
                
                Label_local_base.Content = path;
            }
        }

        private void Загрузка(object sender, RoutedEventArgs e)
        {
            ClearRation();
            first_start = false;

            if (MDL.SetApp.SqlType == (int)ISqlProfile.TypeSqlConnection.Sqlite) {               
                RadioSqlite.IsChecked = true;
                GrindSqlite.Height = 168;
                GrindSqlite.Background = MDL.BrushConv(SELECT_FORM);
            }
            else if (MDL.SetApp.SqlType == (int)ISqlProfile.TypeSqlConnection.MSSQL)
            {
                RadioMsSql.IsChecked = true;
                GrindSqlite.Height = 168;
                Grind_msSql.Background = MDL.BrushConv(SELECT_FORM);
            }

            if (MDL.MyDataBase.CompanyOwn is not null) TBox_Company.Text = MDL.MyDataBase.CompanyOwn.ShortName;

            // sqlite
            Tbox_Filename.Text = MDL.SetApp.DataBaseFile;
            Label_local_base.Content = MDL.SetApp.LocalDir;


          
        }

        private void ClearRation()
        {
            first_start = false;
            GrindSqlite.Height = 38;
            Grind_msSql.Height = 38;
            Radio1.IsChecked = false;
            RadioMsSql.IsChecked = false;
            RadioSqlite.IsChecked = false;
            GrindSqlite.Background = null;
            Grind_msSql.Background = null;
        }

        private void ВыборТипаПодключения(object sender, RoutedEventArgs e)
        {
            if (sender is not RadioButton radio) return;

            if (first_start == false) {
                first_start = true;
                return;
            }  

            if (radio.IsChecked ==true & radio.Name == nameof(RadioSqlite))
            {                
                ClearRation();
                GrindSqlite.Height = 168;
                RadioSqlite.IsChecked = true;
                MDL.SetApp.SqlType = (int)ISqlProfile.TypeSqlConnection.Sqlite;
                GrindSqlite.Background = MDL.BrushConv(SELECT_FORM);          
            }
            else if (radio.IsChecked == true & radio.Name == nameof(Radio1)) {                
                ClearRation( );
                MDL.SetApp.SqlType = (int)ISqlProfile.TypeSqlConnection.none;
                Radio1.IsChecked = true;
                MDL.SetApp.SetProfile();
            }
            else if (radio.IsChecked == true & radio.Name == nameof(RadioMsSql))
            {                
                ClearRation();
                Grind_msSql.Height = 168;
                MDL.SetApp.SqlType = (int)ISqlProfile.TypeSqlConnection.MSSQL;
                RadioMsSql.IsChecked = true;
                MDL.SetApp.SetProfile();
                Grind_msSql.Background = MDL.BrushConv(SELECT_FORM);
            }
        }

        private void Округление_зарплат(object sender, RoutedEventArgs e)
        {
            if (first_start)
            {
                if (sender is RadioButton radio)
                {
                    MDL.SetApp.RoundingAmountsEmpl = radio.IsChecked != false;
                    MDL.SaveSettingApp();
                }
            }
        }

        private void Создать_профиль_msSql(object sender, RoutedEventArgs e)
        {

        }

        private void СортировкаРазмеров(object sender, RoutedEventArgs e)
        {
            if (first_start)
            {
                if (sender is RadioButton radio)
                {
                    MDL.SetApp.SortSizeWood = radio.IsChecked != false;
                    MDL.SaveSettingApp();
                }
            }
        }

        private void ВыборКомпании(object sender, RoutedEventArgs e)
        {
            Pages.ToolList <Model.Catalog.Company> toolList = new(StartGrid, (obj) =>
            {
                if (obj is Model.Catalog.Company comp)
                {
                    TBox_Company.Text = comp.ShortName;
                    MDL.MyDataBase.CompanyOwn = comp;
                }
            });
        }
    }
}
