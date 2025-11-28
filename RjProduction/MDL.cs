using RjProduction.Model;
using RjProduction.Model.Catalog;
using RjProduction.Model.DocElement;
using RjProduction.Sql;
using RjProduction.WpfFrm;
using System.Diagnostics;
using System.Globalization;
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
        /// Необходим для перехвата клавишь для frame
        /// </summary>
        [XmlIgnore] static public Pages.IKeyControl? HandleKey { get; set; }
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
        static public readonly string SFile_DB = AppDomain.CurrentDomain.BaseDirectory + @"Data\db.xml";
        /// <summary>
        /// Каталог ресурсов
        /// </summary>
        static public readonly string Dir_Resources = AppDomain.CurrentDomain.BaseDirectory + @"Resources\";
        /// <summary>
        /// Справочник
        /// </summary>
        static public Reference MyDataBase { get; set; } = new();
        /// <summary>
        /// Настройки программы
        /// </summary>
        static public SettingAppClass SetApp { get; set; } = new();
        /// <summary>
        /// Профиль  для sql подключения
        /// </summary>            
        [XmlIgnore] static public ISqlProfile? SqlProfile { get; set; }
        /// <summary>
        /// Управляет открытими окнами для все открытых WpfView
        /// </summary>
        [XmlIgnore] static public List<WpfView> AllWpfViewWin { get; set; } = [];


        /// <summary>
        /// Управляет страницами основного фрейма добавить
        /// </summary>
        static public void Organizer_Frame_Add(Page page)
        {
            if (MainWindow is null) return;
            MainWindow.OrganizerShowPage(page);
        }
        /// <summary>
        /// Управляет страницами основного фрейма удалить
        /// </summary>
        static public void Organizer_Frame_Delete()
        {
            if (MainWindow is null) return;
            MainWindow.OrganizerClose();
        }

        public static void LogError(string mess, string error_text = "")
        {
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
                if (property != null)
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
                else if (item is CheckBox checkBox) convers(checkBox.Uid, checkBox.IsChecked.ToString() ?? "False");
            }

            return (T)obj;
        }
        /// <summary>
        /// Заполняет поля в Grid из структуры. Именами могут быть поля и свойства их название указываться в Uid 
        /// </summary>
        /// <typeparam name="T">Тип структуры</typeparam>
        /// <param name="grid">Поиск только на этом гриде</param>
        /// <param name="st">сама структура по которой нужно заполнить </param>
        static public void SetStruct<T>(Grid grid, T st) where T : struct
        {

            foreach (var item in grid.Children)
            {
                string? value = "";
                if (item is UIElement ui)
                {
                    var fild = st.GetType().GetField(ui.Uid);
                    if (fild != null)
                    {
                        if (fild.GetValue(st) != null) value = fild.GetValue(st)!.ToString();
                    }
                    else
                    {
                        var pro = st.GetType().GetProperty(ui.Uid);
                        if (pro != null)
                        {
                            if (pro.GetValue(st) != null) value = pro.GetValue(st)!.ToString();
                        }
                    }
                }

                if (string.IsNullOrEmpty(value) == false)
                {
                    if (item is TextBox box) box.Text = value;
                    else if (item is ComboBox cbox) cbox.Text = value;
                    else if (item is Label label) label.Content = value;
                    else if (item is CheckBox checkBox) checkBox.IsChecked = bool.Parse(value);
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
        [DependentCode]
        static public List<IDocMain>? GetDocuments(int year, int month, string doc_code)
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
                    else if (tag == DocCode.Списание_Продукции & doc_code == tag)
                    {
                        idoc = XML.XmlProtocol.LoadDocXML<XML.DocWriteDowns>(file);
                        if (idoc is not null) docs.Add(idoc);
                    }
                    else if (tag == DocCode.Продажи & doc_code == tag)
                    {
                        idoc = XML.XmlProtocol.LoadDocXML<XML.DocSales>(file);
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
                if (t is TextBox text)
                {
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
        static public T ExportFromWpf<T>(FrameworkElement page, T obj) where T : SqlParam
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
        /// Вычисляет количекство вводимое в текстбокс
        /// </summary>
        /// <param name="txt">текст из текстбокса</param>
        /// <returns>решение если с ошибками будет возрат прошлой строки</returns>
        public static string Calculator(TextBox txtbox)
        {
            string txt = txtbox.Text.Replace(" ", "");
            // Поддерживаемые операторы
            char[] value = ['+', '-', '*', '/'];
            char[] operators = value;

            // Находим оператор в строке
            char? operation = null;
            int operatorIndex = -1;

            foreach (char op in operators)
            {
                operatorIndex = txt.IndexOf(op);
                if (operatorIndex != -1)
                {
                    operation = op;
                    break;
                }
            }

            if (operation == null || operatorIndex == -1) return txt;

            // Разделяем строку на левый и правый операнды
            string leftOperandStr = txt[..operatorIndex];
            string rightOperandStr = txt[(operatorIndex + 1)..];

            // Преобразуем операнды в числа
            if (!double.TryParse(leftOperandStr, out double leftOperand) ||
                !double.TryParse(rightOperandStr, out double rightOperand)) return txt;

            // Выполняем операцию
            switch (operation)
            {
                case '+':
                    return (leftOperand + rightOperand).ToString();
                case '-':
                    return (leftOperand - rightOperand).ToString();
                case '*':
                    return (leftOperand * rightOperand).ToString();
                case '/':
                    if (rightOperand != 0) return (leftOperand / rightOperand).ToString();
                    else return txt;
                default:
                    return txt;
            }
        }

        /// <summary>
        /// Создает путь к файлу с учетом создания каталогов по месяцам
        /// </summary>
        /// <param name="dataCreate">дата</param>
        /// <returns>полный путь </returns>
        public static string XmlPatch(DateOnly dataCreate)
        {
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
            if (AllWpfViewWin.Count == 0 & MainWindow.Pages .Count==0)
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
                    Height = 25,
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

            foreach (var page in MainWindow.Pages)
            {
                var b = new Button()
                {
                    Content = string.Concat("Документ ", page.GetHashCode().ToString().AsSpan(0, 3)), 
                    ToolTip= page.Title,
                    Width = 90,
                    Height = 25,
                    MaxWidth = 100,
                    Tag = page,
                    FontSize = 12,
                    Margin = new Thickness(5,0,0,0),
                    Background = null,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontFamily = new FontFamily("Calibri"),
                    BorderBrush =null,
                    FontWeight = FontWeights.Normal
                };
                b.Click += Page_Click;
                MainWindow.StBar.Children.Add(b);
            }

            foreach (var item in MainWindow.StBar.Children)
            {
                if (item is Button button)
                {
                    if (button.Tag is WpfView w)
                    {
                       if ( w.Focus()) button.FontWeight = FontWeights.Bold;
                    }
                }
            }
        }

        private static void Page_Click(object sender, RoutedEventArgs e)
        {
            Page? page = ((Button)sender).Tag as Page;
            MainWindow!.OrganizerShowPage(page!);
        }

        /// <summary>
        /// Добавить сообщение нотификации 
        /// </summary>
        /// <param name="msg">Текст сообщения</param>
        public static void AddNotification(string msg) => MainWindow?.AddNotification(new NotifMessage(msg));

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
        /// Загружает данные из XML. Пропускает если файла нет может быть null если несовпадают типы
        /// </summary>
        /// <typeparam name="T">Тип есть проверка на соответствие</typeparam>
        /// <param name="sFile">Файл проверка на существание если нет файла null</param>
        /// <returns></returns>
        static public T? LoadXml<T>(string sFile)
        {
            if (File.Exists(sFile) == false) return default;
            XmlSerializer xmlSerializer = new(typeof(T));
            try
            {
                using FileStream fs = new(sFile, FileMode.OpenOrCreate);
                object? obj = xmlSerializer.Deserialize(fs);
                if (obj is T t) return t;
                else return default;
            }
            catch (InvalidOperationException ax)
            {
                MessageBox.Show("LoadXml Ошибка чтения файла Xml (" + sFile + ") причина : " + ax.Message);
                return default;
            }
        }

        /// <summary>
        /// Сохранить настройки приложения
        /// </summary>
        static public void SaveSettingApp()
        {
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
            public void LoadValue(string[] strings)
            {
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

            public string SetValueArr(string name, string value, string valueArr)
            {
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
            public void OpenReport()
            {
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

            public List<Contract> Contracts = [];
            public List<Model.WarehouseClass> Warehouses = [];
            public List<MaterialObj> MaterialsDic = [];
            public List<Employee> EmployeeDic = [];
            public List<string> NamesGrup = [];
            /// <summary>
            /// Прошлый номер документа 
            /// </summary>
            public uint NumberDef = 0;
            /// <summary>
            /// Компания выбранная как наша компания  
            /// </summary>
            public Company? CompanyOwn;
            /// <summary>
            /// Склад по умолчанию выбран
            /// </summary>
            public Model.WarehouseClass? WarehouseDef;
            /// <summary>
            /// Выбранный профиль зарплат для рабочих
            /// </summary>
            public SalaryRatesClass SalaryRates { get; set; } = new();




            /// <summary>
            /// Профиль зарплат для работников
            /// </summary>
            public class SalaryRatesClass
            {
                public double Price_Up { get; set; }
                public double Price_Down { get; set; }
                public double Price_Standart { get; set; }
                public byte Proc_Price_Up { get; set; } = 5;
                public byte Proc_Price_Down { get; set; } = 5;
            }

            /// <summary>
            /// Сохранить базу данных в файл переменной MyDataBase
            /// </summary>
            public static void SaveDB() => MDL.SaveXml<MDL.Reference>(MDL.MyDataBase, MDL.SFile_DB);

            /// <summary>
            /// Доступ к отдельным файлам спискам данных
            /// </summary>
            public readonly struct Catalog<T>
            {
                private readonly string _sFile;
                /// <summary>
                /// Основный список из файла
                /// </summary>
                public readonly List<T> ListCatalog;

                /// <summary>
                /// Загрузить данные из BaseDirectory > Data   typeof(T).Name.xml Сразу после вызова контруктора
                /// </summary>
                /// <exception cref="InvalidOperationException">Ошибка чтения файла Xml</exception>
                public Catalog()
                {
                    _sFile = AppDomain.CurrentDomain.BaseDirectory + "Data\\" + typeof(T).Name + ".xml";

                    if (File.Exists(_sFile))
                    {
                        XmlSerializer xmlSerializer = new(typeof(List<T>));
                        try
                        {
                            using FileStream fs = new(_sFile, FileMode.OpenOrCreate);
                            object? obj = xmlSerializer.Deserialize(fs);
                            if (obj is List<T> t) ListCatalog = t;
                            else ListCatalog = [];
                        }
                        catch (InvalidOperationException ax)
                        {
                            MessageBox.Show("Ошибка чтения файла Xml (" + _sFile + ") причина : " + ax.Message);
                            ListCatalog = [];
                        }
                    }
                    else
                    {
                        ListCatalog = [];
                    }
                }

                public void SaveData() => MDL.SaveXml<List<T>>(ListCatalog, _sFile);

                /// <summary>
                /// Поиск существующей строки сравнение из ToString() метода
                /// </summary>
                /// <returns>true - есть вариация такой строки</returns>
                public bool ExistItem(string str)
                {
                    foreach (var item in ListCatalog)
                    {
                        if (string.IsNullOrEmpty(item!.ToString())) continue;
                        else if (str.ToLower().Trim() == item.ToString()!.ToLower().Trim()) return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Настройки для программы
        /// </summary>
        public class SettingAppClass
        {
            public readonly string SetFile = AppDomain.CurrentDomain.BaseDirectory + @"Data\Setting_App.xml";

            public string LocalDir;
            public string DataBaseFile;
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
            /// Сортировать размеры пиломатериала в измежание дубликатов
            /// </summary>
            public bool SortSizeWood { get; set; } = true;
            /// <summary>
            /// Округялет суммы рабочим без копеек
            /// </summary>
            public bool RoundingAmountsEmpl { get; set; } = true;
            /// <summary>
            /// Имя пользователя 
            /// </summary>
            public string UserName { get; set; } = "TestApp";

        }

        /// <summary>
        /// Иконка сообщения нотификации 
        /// </summary>
        public class NotifMessage
        {
            private readonly Grid MyGrid;
            private readonly Label TitelSimbol;
            private readonly ProgressBar PBar;
            private readonly string Msg;

            public delegate void EventClose(NotifMessage grid);
            public event EventClose? Close;

            public NotifMessage(string msg)
            {
                Msg = msg;
                MyGrid = new Grid();
                PBar = new ProgressBar
                {
                    Value = 1,
                    Orientation = Orientation.Vertical,
                    Width = 34,
                    Height = 34,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x5D, 0x6B, 0x99)),
                    Background = null,
                    BorderBrush = null
                };
                TitelSimbol = new Label
                {
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Content = "\uE134",
                    FontSize = 20,
                    Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x52, 0x2D, 0xAE)),
                    Background = null
                };
                MyGrid.Children.Add(PBar);
                MyGrid.Children.Add(TitelSimbol);
            }

            private void ButtonExit_Click(object sender, RoutedEventArgs e)
            {
                Close?.Invoke(this);
            }

            public Button CreateInstance()
            {
                return new Button
                {
                    Width = 34,
                    Height = 34,
                    Margin = new Thickness(5, 0, 0, 2),
                    BorderBrush = null,
                    Tag = this,
                    Background = null,
                    Content = MyGrid,
                    ToolTip = Msg
                };
            }


            public async Task Start()
            {
                while (PBar.Value < PBar.Maximum)
                {
                    await Task.Delay(150);
                    Application.Current.Dispatcher.Invoke(() => PBar.Value += 1);
                }
                ButtonExit_Click(null!, null!);
            }


        }
    }
}
