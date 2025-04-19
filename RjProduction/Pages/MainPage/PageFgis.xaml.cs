using RjProduction.Fgis;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace RjProduction.Pages.MainPage
{
    public partial class PageFgis : Page
    {
        public Config_fgis Config = new Config_fgis();

        public PageFgis()
        {
            InitializeComponent();
        }

        private void ОткрытьОт(object sender, RoutedEventArgs e)
        {
            
        }

        private void НовыйДок(object sender, RoutedEventArgs e)
        {
            if (Config.Status == false) {
                MessageBox.Show("С начало нужно обновить справочники из сети на новую версию");
                return;
            }
            DockPanel_РамкаДокумента.Visibility = Visibility.Visible;
            FrameDisplay.Navigate(new Fgis.Pages.UsageReport(new(), Config));
        }

        /// <summary>
        /// Содержит настройки для загрузки справочников для fgis
        /// </summary>
        public class Config_fgis
        {
            private string Dirs = AppDomain.CurrentDomain.BaseDirectory + @"Res\FGIS\";

            private bool _status = false;
            private string _Common = string.Empty;
            private string _Ver = "0";
            private string _ForestUsageReport = string.Empty;

            public string Common { get => Dirs + _Common; }
            public string ForestUsageReport { get => Dirs + _ForestUsageReport; }
            public string Ver { get => _Ver; }

            public bool Status { get => _status; }  

            public Config_fgis()
            {
                string cnf = Dirs + "config.ini";
                if (File.Exists(cnf) == false) throw new FileNotFoundException("Файл конфигурации не найден " + cnf);
                string[] txt = System.IO.File.ReadAllLines(cnf, System.Text.Encoding.Default);

                System.Reflection.FieldInfo? p;
                try
                {
                    foreach (var item in txt)
                    {
                        string nameType = "_" + item.Split('=')[0];       // Название переменной
                        string param = item.Split('=')[1];  // Значение истинное или нет
                        p = typeof(Config_fgis).GetField(nameType, System.Reflection.BindingFlags.Instance |
                                                            System.Reflection.BindingFlags.Public |
                                                            System.Reflection.BindingFlags.NonPublic );
                        p?.SetValue(this, param);
                    }
                   _status= _Common != string.Empty ;
                }
                catch
                {
                    throw new Exception("Error reading file " + cnf + "\n PropertyInfo ");
                }

            }
        }

        private void Загрузка(object sender, RoutedEventArgs e)
        {
            /// проверка на ошибки или отсутствие  справочника           
            if (Config.Status == true ) ErrorUpdate.Visibility= Visibility.Collapsed;

        }

        private void ВыборССылки(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //https://rosleshoz.gov.ru/doc/common_fgis_lk
        }
    }
}
