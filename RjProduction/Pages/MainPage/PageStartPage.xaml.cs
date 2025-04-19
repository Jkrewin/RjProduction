
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;

namespace RjProduction.Pages
{    
    public partial class PageStartPage : Page
    {
        public PageStartPage()
        {
            InitializeComponent();
        }

        private void ЗагрузкаПрограммы(object sender, RoutedEventArgs e)
        {

            if (MDL.SqlProfile is not null)
            {
                if (Sql.SqlRequest.TestingDB()==false) G_db_Error.Visibility = Visibility.Visible;
            }


            UpdateSoft();// Проверка на обновление 





        }
        /// <summary>
        /// <code>файл.txt должен содержать команды </code>
        /// </summary>
        private void UpdateSoft() {
            string dir = AppContext.BaseDirectory + "Update";

            foreach (var file in Directory.GetFiles(dir))
            {
                FileInfo fileInfo = new(file);
                if (fileInfo.Extension != ".txt") continue;
                string[] tag = fileInfo.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tag[0] == "update")
                {
                    var v1 = Assembly.GetExecutingAssembly().GetName().Version ?? new Version();
                    tag[1] = tag[1].Replace('_', '.');
                    tag[1] = tag[1][1..^4];
                    var v2 = new Version(tag[1]);

                    if (v1 >= v2)
                    {
                        G_Updater.Visibility = Visibility.Visible;
                        Button_Обновление.Uid = fileInfo.FullName;
                    }
                }
            }
        }


        private void ОбновлениеПрограммы(object sender, RoutedEventArgs e)
        {
            string str = Button_Обновление.Uid;

            Dictionary<string, string> dic = [];
            string deep = "";
            string com = "";

            // Сборка скриптов в группу
            foreach (var item in File.ReadAllLines(str, System.Text.Encoding.Default))
            {
                if (com != "")
                {
                    if (item.Contains('}'))
                    {
                        deep += "\n" + item.Split('}')[0];
                        dic.Add(com, deep);
                        com = "";
                    }
                    else
                    {
                        deep +="\n"+ item;
                    }
                }
                else
                {
                    string s = item.TrimStart();
                    if (s.Length > 3 && s[0..4] == "#sql")
                    {
                        com = "#sql";
                        deep = item.Split('{')[1];
                    }
                }
            }

            if (MDL.SqlProfile is null)
            {
                MessageBox.Show("Необходимо подключить профили к базе данных, в настройках программы");
                return;
            }
            // процесс запуска скрипта
            MDL.SqlProfile.Conection();
            try
            {
                foreach (var item in dic)
                {
                    if (item.Key == "#sql")
                    {
                        MDL.SqlProfile.SqlCommand(item.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                MDL.LogError("Ошибка при выполнение скрипта ", ex.Message + " ==>" + MDL.SqlProfile.SqlLogString);
                return;
            }
            finally
            {
                MDL.SqlProfile.Disconnect();
            }

            System.IO.File.Move(str, str + ".old");

            G_Updater.Visibility = Visibility.Collapsed;
        }

        private void ПереходНаСайт(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            };
            e.Handled = true;
            Process.Start(psi);
        }
    }
}
