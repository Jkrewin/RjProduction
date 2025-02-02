using RjProduction.Model;
using RjProduction.Sql;
using RjProduction.WpfFrm;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;
using static RjProduction.Sql.ISqlProfile;

namespace RjProduction
{
    static public class MDL
    {
        /// <summary>
        /// Основное окно получает стандартное оформление, верхнаяя часть панели меняеться на системную
        /// </summary>
        static public bool WindowsStandart { get; set; } = false;
       /// <summary>
       /// Основная форма
       /// </summary>
       static public MainWindow? MainWindow { get; set; }
        /// <summary>
        /// Текущий каталог где справочник
        /// </summary>
        static public readonly string SFile_DB = AppDomain.CurrentDomain.BaseDirectory + @"Res\db.xml";
        /// <summary>
        /// Справочник
        /// </summary>
        static public Reference MyDataBase { get; set; } = new();
        /// <summary>
        /// Настройки программы
        /// </summary>
        static public SettingAppClass SetApp {get;set;} =new ();
        /// <summary>
        /// Профиль  для sql подключения
        /// </summary>            
        [XmlIgnore] static public ISqlProfile? SqlProfile { get; set; }
        /// <summary>
        /// Управляет открытими окнами для все открытых WpfView
        /// </summary>
        [XmlIgnore] static public List<WpfView> AllWpfViewWin { get; set; } = [];

        public static void LogError(string mess, string error_text="") {
            var t = new WpfFrm.ErrorLog(mess, error_text);
            t.ShowDialog();
        }
        /// <summary>
        /// Uses BrushConverter to convert text to color. 
        /// </summary>
        /// <param name="col">Format ex = #FF4B566A</param>
        /// <returns>Media.Brush</returns>
        public static System.Windows.Media.Brush? BrushConv(string col)
        {
            var bc = new System.Windows.Media.BrushConverter();
            return (System.Windows.Media.Brush?)bc.ConvertFrom(col);
        }
        /// <summary>
        /// Заполняет  структуру из Grid
        /// </summary>
        /// <typeparam name="T">тип структуры</typeparam>
        /// <param name="grid">Grid для поиска</param>
        /// <returns>Получает заполненая структура</returns>
        /// <exception cref="Exception">некоторые элемены типа данных нужно зарегистрировать если их нет</exception>
        static public T GetStruct<T>(Grid grid) where T : struct
        {           
            object obj = default(T);

            void convers(string uid, string value)
            {
                if (string.IsNullOrEmpty(value) | string.IsNullOrEmpty(uid)) return;               
                var property = obj.GetType().GetProperty(uid);
                    if (property!=null)
                    {
                        if (property.PropertyType.IsEnum)
                        {
                            property.SetValue(obj, Enum.Parse(property.PropertyType, value));
                            return;
                        }
                        switch (property.PropertyType.Name)
                        {
                            case "Double":
                                property.SetValue(obj, double.Parse(value)); break;
                            case "String":
                                property.SetValue(obj, value); break;
                            case "Boolean":
                            if (value != "False") property.SetValue(obj, true);
                            else property.SetValue(obj, false);
                            break;
                        default:
                                throw new Exception("Такой тип не зарегестрирован " + property.PropertyType.ToString());
                        }
                    }               
            }

            foreach (var item in grid.Children)
            {
                if (item is TextBox box) convers(box.Uid, box.Text);
                else if (item is ComboBox cbox) convers(cbox.Uid, cbox.Text);
                else if (item is Label label) convers(label.Uid, label.Content.ToString()!);
                else if (item is CheckBox checkBox) convers(checkBox.Uid,checkBox.IsChecked.ToString() ?? "False");
            }

            return (T)obj;
        }
        /// <summary>
        /// Заполняет поля в Grid из структуры. Именами могут быть поля и свойства их название указываться в Uid 
        /// </summary>
        /// <typeparam name="T">Тип структуры</typeparam>
        /// <param name="grid">Поиск только на этом гриде</param>
        /// <param name="st">сама структура по которой нужно заполнить </param>
        static public void SetStruct<T>(Grid grid, T st) where T : struct {
                 
            foreach (var item in grid.Children)
            {
                string? value="";
                if (item is UIElement ui) {
                    var fild = st.GetType().GetField(ui.Uid);
                    if (fild != null)
                    {
                        if (fild.GetValue(st) != null) value = fild.GetValue(st)!.ToString() ;
                    }
                    else {
                        var pro = st.GetType().GetProperty(ui.Uid);
                        if (pro != null) {
                           if (pro.GetValue(st) !=null) value = pro.GetValue(st)!.ToString() ;
                        }
                    }
                }

                if ( string.IsNullOrEmpty(value)==false) {
                    if (item is TextBox box) box.Text = value;
                    else if (item is ComboBox cbox) cbox.Text = value;
                    else if (item is Label label) label.Content = value;
                    else if (item is CheckBox checkBox)  checkBox.IsChecked = bool.Parse(value);
                }   
            }
        }
        /// <summary>
        /// Загружает в список данные из файлов за <b>текущий</b> период
        /// </summary>
        /// <returns></returns>
        static public List<IDocMain>? GetDocuments(string doc_code) => GetDocuments(DateTime.Now.Year, DateTime.Now.Month, doc_code);
        /// <summary>
        /// Загружает в список данные из файлов за выбранный период
        /// </summary>
        /// <param name="year">год</param>
        /// <param name="month">месяц</param>
        /// <returns></returns>
        [DependentCode]static public List<IDocMain>? GetDocuments(int year, int month, string doc_code )
        {
            bool allset = doc_code == "";
            List<IDocMain> docs = [];
            string sfile = AppDomain.CurrentDomain.BaseDirectory + $"xmldocs\\{year}\\{month}\\";
            try
            {
                foreach (var file in Directory.GetFiles(sfile))
                {
                    FileInfo fileInfo = new(file);
                    string tag = fileInfo.Name.Split('_', StringSplitOptions.RemoveEmptyEntries)[0];
                    if (allset) doc_code = tag;
                    IDocMain? idoc;
                    if (tag == DocCode.Производство_Cклад & doc_code == tag)
                    {
                        idoc = XML.XmlProtocol.LoadDocXML<XML.DocArrival>(file);
                        if (idoc is not null) docs.Add(idoc);
                    }
                    else if (tag == DocCode.Выравнивание_Остатков & doc_code == tag)
                    {
                        idoc = XML.XmlProtocol.LoadDocXML<XML.DocShipments>(file);
                        if (idoc is not null) docs.Add(idoc);
                    }
                    else if (tag == DocCode.Перемещение_По_Складам & doc_code == tag)
                    {
                        idoc = XML.XmlProtocol.LoadDocXML<XML.DocMoving>(file);
                        if (idoc is not null) docs.Add(idoc);
                    }
                }
            }
            catch { return null; }

            return docs;
        }
        /// <summary>
        /// Заполняет данными класса форму или страницу
        /// </summary>
        static public void ImportToWpf(FrameworkElement page, object obj)
        {
            foreach (var item in obj.GetType().GetProperties())
            {
                var t = page.FindName(item.Name);
                if (t is TextBox text) {
                    var o = item.GetValue(obj);
                    if (o is not null) text.Text = o.ToString();
                    else text.Text = string.Empty;
                }
                else if (t is Label lab) lab.Content = item.GetValue(obj);
                else if (t is DatePicker dat)
                {
                    dat.DisplayDate = ((DateOnly)item.GetValue(obj)!).ToDateTime(TimeOnly.MinValue);
                    dat.Text = dat.DisplayDate.ToString();
                }
                else if (t is CheckBox checkBox)
                {
                    if (bool.TryParse(item.GetValue(obj)!.ToString() ?? "false", out bool b))
                    {
                        checkBox.IsChecked = b;
                    }
                    else checkBox.IsChecked = false;
                }
            }
        }       
        /// <summary>
        /// Выгружает страницу или форму на нужный класс заполняе его. Выгружает страницу или форму на нужный класс заполняе только для SqlParam наслединков 
        /// </summary>
        static public T ExportFromWpf<T>(FrameworkElement page, T obj)  where T : SqlParam
        {          
            foreach (var property in obj!.GetType().GetProperties())
            {
                if (property.CanWrite == false) continue;
                var t = page.FindName(property.Name);
                if (t is null) continue;
                string refObj = "";

                if (t is TextBox text) refObj = text.Text;
                else if (t is Label lab) refObj = lab.Content.ToString() ?? string.Empty;
                else if (t is DatePicker dat) refObj = dat.DisplayDate.ToString();
                else if (t is CheckBox checkBox) refObj = checkBox.IsChecked.ToString() ?? "false";


                if (property.PropertyType.IsEnum)
                {
                    property.SetValue(obj, Enum.Parse(property.PropertyType, refObj));
                    continue;
                }

                switch (property.PropertyType.Name)
                {
                    case "DateTime":
                        property.SetValue(obj, DateTime.Parse(refObj));
                        break;
                    case "Double":
                        property.SetValue(obj, Double.Parse(refObj));
                        break;
                    case "Int16":
                        property.SetValue(obj, Int16.Parse(refObj));
                        break;
                    case "Int32":
                        property.SetValue(obj, Int32.Parse(refObj));
                        break;
                    case "Int64":
                        property.SetValue(obj, Int64.Parse(refObj));
                        break;
                    case "Decimal":
                        property.SetValue(obj, decimal.Parse(refObj));
                        break;
                    case "Boolean":
                        bool b;
                        if (Boolean.TryParse(refObj, out b) == false)
                        {
                            if (refObj == "1") b = true;
                        }
                        property.SetValue(obj, b);
                        break;
                    default:
                        property.SetValue(obj, refObj);
                        break;
                }
            }
            return (T)obj;
        }

        /// <summary>
        /// Создает путь к файлу с учетом создания каталогов по месяцам
        /// </summary>
        /// <param name="dataCreate">дата</param>
        /// <returns>полный путь </returns>
        public static string XmlPatch(DateOnly dataCreate) {
            string sFile = AppDomain.CurrentDomain.BaseDirectory + "xmldocs\\";
            sFile += dataCreate.Year.ToString();
            if (!File.Exists(sFile)) Directory.CreateDirectory(sFile);
            sFile += "\\";
            sFile += dataCreate.Month.ToString();
            if (!File.Exists(sFile)) Directory.CreateDirectory(sFile);
            return sFile;
        }
        /// <summary>
        /// обновить список окон и показать UI какие окна используються
        /// </summary>
        public static void Refreh_AllWpfView()
        {
            if (MainWindow is null) return;
            if (AllWpfViewWin.Count == 0)
            {
                MainWindow.StBar.Visibility = Visibility.Collapsed;
                MainWindow.MainGrind.RowDefinitions[1].Height = new GridLength(1);
                return;
            }
            else
            {
                MainWindow.MainGrind.RowDefinitions[1].Height = new GridLength(24);
                MainWindow.StBar.Visibility = Visibility.Visible;
            }

            MainWindow.StBar.Children.Clear();
            foreach (var item in AllWpfViewWin)
            {
                var b = new Button()
                {
                    Content = item.Title,
                    Width = 100,
                    Height=25,
                    MaxWidth = 100,
                    Tag = item,
                    FontSize = 10,
                    FontFamily = new FontFamily("Arial"),
                    FontWeight = FontWeights.Normal
                };
                b.Click += B_Click;
                item.Activated += Item_Activated;
                MainWindow.StBar.Children.Add(b);
            }

            foreach (var item in MainWindow.StBar.Children)
            {
                if (item is Button button)
                {
                    if (((WpfView)button.Tag).Focus()) {
                        button .FontWeight = FontWeights.Bold;
                    }
                }
            }
        }

        private static void Item_Activated(object? sender, EventArgs e)
        {
            if (MainWindow is null) return;
            foreach (var item in MainWindow.StBar.Children)
            {
                if (item is Button button)
                {
                    if (((WpfView)button.Tag).Equals(sender)) button.FontWeight = FontWeights.Bold;
                    else button.FontWeight = FontWeights.Normal;
                }
            }
        }

        private static void B_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                ((Window)b.Tag).Focus();
            }
        }

        /// <summary>
        /// Сохрание файла xml с полной очисткой файла
        /// </summary>
        static public void SaveXml<T>(T cl, string sFile)
        {
            XmlSerializer xmlSerializer = new(typeof(T));
            try
            {
                FileStream fs = new(sFile, FileMode.Create, FileAccess.Write, FileShare.None);
                try
                {
                    fs.SetLength(0);
                    xmlSerializer.Serialize(fs, cl);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                MDL.LogError($"Ошибка при сохрании в файл {cl}", sFile + "\n   " + ex.Message + "\n" + ex.Source + "\n" + ex.InnerException);
            }
        }
        /// <summary>
        /// Сохранить настройки приложения
        /// </summary>
        static public void SaveSettingApp() {
            MDL.SaveXml<MDL.SettingAppClass>(MDL.SetApp, MDL.SetApp.SetFile);
        }
                

        /// <summary>
        /// Создает отчет на основе html страницы.
        /// Требует наличии заменямых значений $так$ и блоки кода тегов $теги и $теги end
        /// </summary>
        public class HtmlReport(string sFile)
        {
            private readonly string SFile = sFile;           
            private readonly StringBuilder _text = new("");

            /// <summary>
            /// Список шаблонов полученных в момент настроки и загруки из файла
            /// </summary>
            public Dictionary<string, string> DicHtml = [];
            /// <summary>
            /// Переменные среды документа. Название и значение
            /// </summary>
            public Dictionary<string, string> ValueArr = [];
            /// <summary>
            /// Добавить текст
            /// </summary>
            public string AddText { get => _text.ToString(); set => _text.Append(value); }

            private static string CutText(string tag, string text)
            {
                int startIndex = text.IndexOf(tag) + tag.Length;
                int endIndex = text.IndexOf(tag + " end", startIndex) - 1;
                return text[startIndex..endIndex];
            }

            /// <summary>
            /// <code>new string[] { "", "" }</code>
            /// </summary>
            /// <param name="strings"></param>
            public void LoadValue(string[] strings ) {              
                if (!File.Exists(SFile))
                {
                    MessageBox.Show("Файл не найден !");
                    return;
                }
                string html = File.ReadAllText(SFile, System.Text.Encoding.UTF8);
                foreach (var item in strings)
                {
                    DicHtml.Add(item, CutText(item, html));
                }               
            }

            /// <summary>
            /// Item1 = переменная будет преоразованна в >> <i>$перемнная$ </i> Item2 = значение которое заменить 
            /// <code> List<(string, string)> values = [];values.Add( new ("a", "b") );</code>
            /// </summary>
            public static void SetValue(string name, string value, ref string text) => text = text.Replace("$" + name + "$", value);

            public string SetValueArr(string name, string value, string valueArr) {
                return ValueArr[valueArr].Replace("$" + name + "$", value);
            }
            /// <summary>
            /// Замена значени через DicHtml
            /// </summary>
            /// <param name="name">имя переменной</param>
            /// <param name="value">значение</param>
            /// <param name="dicHtml">название заголовка из DicHtml </param>
            /// <returns>результат строки</returns>
            public string SetValueStr(string name, string value, string dicHtml)
            {
                return DicHtml[dicHtml].Replace("$" + name + "$", value);
            }

            /// <summary>
            /// Загрузка переменных из DicHtml такого формата $FFxx...
            /// </summary>
            /// <param name="dicHtmlName"></param>
            public void LoadArr(string dicHtmlName)
            {
                string html = DicHtml[dicHtmlName];
                foreach (var item in html.Split("$FF", StringSplitOptions.RemoveEmptyEntries))
                {
                    int index = item.IndexOf('=');
                    if (index != -1)
                    {
                        string key = string.Concat("FF", item.AsSpan(0, index));
                        ValueArr.Add(key, item[(index + 1)..]);
                    }
                }
            }

            /// <summary>
            /// Показать собранный отчет
            /// </summary>
            public void OpenReport() {
                string endFile = AppDomain.CurrentDomain.BaseDirectory + @"Res\Отчет.html";
                File.WriteAllText(endFile, _text.ToString(), System.Text.Encoding.Default);
                Process.Start(new ProcessStartInfo(endFile) { UseShellExecute = true });
            }
        }

        /// <summary>
        /// Справочник
        /// </summary>
        public class Reference
        {
            private List<string> _employeeDic = [];

            public List<Model.WarehouseClass> Warehouses = [];
            public List<Model.MaterialObj> MaterialsDic = [];
            public List<string> EmployeeDic
            {
                get => _employeeDic;
                set => _employeeDic = value;
            }
            public List<string> NamesGrup = [];
            /// <summary>
            /// Прошлый номер документа 
            /// </summary>
            public uint NumberDef = 0;
          
            /// <summary>
            /// Склад по умолчанию выбран
            /// </summary>
            public Model.WarehouseClass? WarehouseDef;
        }

        /// <summary>
        /// Настройки для программы
        /// </summary>
        public class SettingAppClass
        {
            public readonly string SetFile = AppDomain.CurrentDomain.BaseDirectory + @"Res\Setting_App.xml";

            public string LocalDir ;
            public string DataBaseFile ;
            public int SqlType = (int)ISqlProfile.TypeSqlConnection.none;

            /// <summary>
            /// Статус главного окна по умолчанию
            /// </summary>
            public WindowState WindowStateDef = WindowState.Normal;

            /// <summary>
            /// Установить профиль
            /// </summary>
            public void SetProfile()
            {
                switch (SqlType)
                {
                    case (int)TypeSqlConnection.none:
                        break;
                    case (int)TypeSqlConnection.MSSQL:
                        break;
                    case (int)TypeSqlConnection.Sqlite:
                        MDL.SqlProfile = new SqliteProfile()
                        {
                            DataBaseFile = DataBaseFile,
                            LocalDir = LocalDir
                        };
                        break;
                    case (int)TypeSqlConnection.MySQL:
                        break;
                    default:
                        break;
                }
            }

            public SettingAppClass()
            {
                SqliteProfile sqlite = new();
                LocalDir = sqlite.LocalDir;
                DataBaseFile = sqlite.DataBaseFile;
            }


            /// <summary>
            /// Округялет суммы рабочим без копеек
            /// </summary>
            public bool RoundingAmountsEmpl { get; set; } = true;
            /// <summary>
            /// Имя пользователя 
            /// </summary>
            public string UserName { get; set; } = "TestApp";
        }
    }
}
