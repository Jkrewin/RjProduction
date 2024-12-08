using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace RjProduction
{
    static public class MDL
    {       
        /// <summary>
        /// Текущий каталог где справочник
        /// </summary>
        static public readonly string SFile_DB = AppDomain.CurrentDomain.BaseDirectory + @"Res\db.xml";
        /// <summary>
        /// Справочник
        /// </summary>
        static public BoardDic MyDataBase { get; set; } = new ();

        static public string UserName { get; set; } = "TestApp";

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
        static public List<Model.Document>? GetDocuments() {           
            return GetDocuments(DateTime.Now.Year, DateTime.Now.Month);
        }
        /// <summary>
        /// Загружает в список данные из файлов за выбранный период
        /// </summary>
        /// <param name="year">год</param>
        /// <param name="month">месяц</param>
        /// <returns></returns>
        static public List<Model.Document>? GetDocuments(int year, int month)
        {
            List<Model.Document> docs = [];
            string sfile = AppDomain.CurrentDomain.BaseDirectory + $"xmldocs\\{year}\\{month}\\";
            try
            {
                foreach (var file in Directory.GetFiles(sfile))
                {
                    docs.Add(XML.XmlDocument.LoadXML(file));
                }
            }
            catch  { return null; }

            return docs;
        }
        /// <summary>
        /// Заполняет данными класса форму или страницу
        /// </summary>
        static public void FullWpf(FrameworkElement page, object obj)
        {
            foreach (var item in obj.GetType().GetProperties())
            {
                var t = page.FindName(item.Name);
                if (t is TextBox text) text.Text = item.GetValue(obj)!.ToString() ?? string.Empty;
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
        /// Сохраняет справочник. 
        /// сохранение файла тут происходит с полной очисткой файла
        /// </summary>
        static public void SaveXml<T>(T cl, string sFile) {
            XmlSerializer xmlSerializer = new(typeof(T));
            try
            {
                FileStream fs = new(sFile, FileMode.Create, FileAccess.Write, FileShare.None);
                try
                {
                    fs.SetLength(0);
                    xmlSerializer.Serialize(fs, cl);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    fs.Close();
                }
            }
            catch
            {
                Console.WriteLine("Ошибка доступа к файлу " + sFile);
            }

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
        public class BoardDic
        {
            private List<string> _employeeDic = [];

            public List<Model.Document.MaterialObj> MaterialsDic = [];
            public List<string> EmployeeDic
            {
                get => _employeeDic;
                set => _employeeDic = [.. value.OrderBy(x => x)];
            }
            public List<string> NamesGrup = [];
            /// <summary>
            /// Прошлый номер документа 
            /// </summary>
            public uint NumberDef = 0;
            /// <summary>
            /// Статус главного окна по умолчанию
            /// </summary>
            public WindowState WindowStateDef = WindowState.Normal;
        }
    }
}
