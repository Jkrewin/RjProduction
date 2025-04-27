using RjProduction.Fgis.XML;
using RjProduction.WpfFrm;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RjProduction.Fgis.WPF
{
    public partial class DialogWindow : Window
    {
        private const string Required_Color = "#FFF87777";
        private const string Optional_Color = "#FF999999";

        private readonly KindTypes _kindTypes;
        private readonly Action<object> _action;
        private readonly object _cl;
        private readonly Dictionary<string, DeliveredStruct> _Dic = [];


        public DialogWindow(object cl, Action<object> action, KindTypes kindTypes)
        {
            InitializeComponent();
            _cl = cl;
            _action = action;
            _kindTypes = kindTypes;
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            Height = 150;
            Label_Title.Content = _cl.GetType().Name;
            foreach (var m in _cl.GetType().GetCustomAttributes(false))
            {
                if (m is Mod mod)
                {
                    Label_Title.Content = mod.Comment;
                }
            }

            Grid obj_ui = (Grid)this.FindName("Grind_End");
            MainPanel.Children.Clear();

            if (_cl is forestUsageReport.IDefault def)
            {
                def.Default(_kindTypes);
            }

            foreach (var item in _cl.GetType().GetProperties())
            {
                object? value = item.GetValue(_cl);
                Mod? mod = Mod.Extract_Mod(item);

                if (mod is not null)
                {
                    DeliveredStruct dl = new("", 0, "");
                    string s;
                    switch (mod.Format)
                    {
                        case Mod.FormatEnum.Email:
                        case Mod.FormatEnum.Text:
                            s = value?.ToString() ?? "--";
                            AddTextBox(mod, item.Name, s);
                            dl = new DeliveredStruct(s, 0, s, s);
                            break;
                        case Mod.FormatEnum.INN:
                        case Mod.FormatEnum.OGRN:
                        case Mod.FormatEnum.OGRN_ip:
                        case Mod.FormatEnum.Ints:
                            s = value?.ToString() ?? "--";
                            AddIntsBox(mod, item.Name, s);
                            dl = new DeliveredStruct(s, 0, s, s);
                            break;
                        case Mod.FormatEnum.Date:
                            s = value?.ToString() ?? "--";
                            AddDateBox(mod, item.Name, value?.ToString() ?? "");
                            dl = new DeliveredStruct(s, 0, s, s);
                            break;
                        case Mod.FormatEnum.Cadastr:

                            dl = new DeliveredStruct("rflfcnh", 0, "изменить", "0");
                            break;
                        case Mod.FormatEnum.List:
                            if (value is not null)
                            {
                                AddList(mod, item.Name, value);
                                dl = new DeliveredStruct("list", 0, "список", value);
                                break;
                            }
                            else throw new Exception("open_obj_cl пустой элемент ");
                        case Mod.FormatEnum.winClass:
                            if (value is not null)
                            {
                                AddOpenClass(mod, item.Name, value);
                                dl = new DeliveredStruct("class", 0, "тип", value);
                            }
                            else
                            {
                                object? open_obj = Activator.CreateInstance(item.PropertyType) ?? throw new Exception("Невозможно создать значение");
                                AddOpenClass(mod, item.Name, open_obj);
                                dl = new DeliveredStruct("class", 0, "тип", open_obj);
                            }
                            break;
                        case Mod.FormatEnum.dictionary:
                            s = value?.ToString() ?? "";
                            AddButton(mod, item.Name, mod.Dic, s);
                            dl = new DeliveredStruct(s, 0, s, s);
                            break;
                    }

                    _Dic.Add(item.Name, dl);
                }
            }
            MainPanel.Children.Add(obj_ui);
        }

        private bool FullClass(object cl)
        {
            bool error = true;
            void col(Brush brush, string name)
            {
                foreach (Grid g in MainPanel.Children)
                {
                    foreach (var ff in g.Children)
                    {
                        if (ff is Control f)
                        {
                            if (f.Uid == name)
                            {
                                f.Background = brush;
                                goto end;
                            }
                        }
                    }
                }
            end:;
            }

            foreach (var item in _cl.GetType().GetProperties())
            {
                Mod? mod = Mod.Extract_Mod(item);
                if (mod is not null)
                {
                    if (mod.Format == Mod.FormatEnum.Hide) continue;
                    if (_Dic.TryGetValue(item.Name, out DeliveredStruct value))
                    {
                        if (Mod.CheckRule(value, mod))
                        {
                            item.SetValue(cl, value.Obj);
                            col(MDL.BrushConv("#FFFFFAF0")!, item.Name);
                        }
                        else
                        {
                            error = false;
                            col(Brushes.MistyRose, item.Name);
                        }
                    }
                }
            }
            return error;
        }

        private static Brush ColorSet(Mod.MTypeEnum mType) => mType switch
        {
            Mod.MTypeEnum.Required => MDL.BrushConv(Required_Color)!,
            Mod.MTypeEnum.Optional => MDL.BrushConv(Optional_Color)!,
            Mod.MTypeEnum.Select => Brushes.GreenYellow,
            Mod.MTypeEnum.Code => Brushes.Blue,
            _ => throw new NotImplementedException(),
        };

        private void AddList(Mod m, string nameObj, object obj)
        {
            Height += 127;

            Grid grid = new()
            {
                Height = 127,
                Background = Brushes.White,
                Width = 800
            };

            Label labelA = new()
            {
                Content = m.Comment,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                FontFamily = new("Microsoft Sans Serif"),
                Background = Brushes.FloralWhite,
                Height = 22,
                Padding = new(5, 3, 5, 5),
                Width = 800,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            Label labelB = new()
            {
                Content = "",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                FontFamily = new("Arial"),
                FontSize = 14,
                Background = ColorSet(m.MType),
                Margin = new(120, 23, 0, 0),
                Height = 104,
                Width = 10
            };

            ListBox lbox = new()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Width = 670,
                FontFamily = new("Arial"),
                Height = 104,
                Tag = obj
            };

            Button buttonA = new()
            {
                Content = "Добавить",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new(35, 27, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 80,
                Background = MDL.BrushConv("#FFDFFDF5"),
                BorderBrush = MDL.BrushConv("#FF0008E3"),
                Height = 24,
                Foreground = MDL.BrushConv("#FF0008E3")
            };

            Button buttonB = new()
            {
                Content = "Удалить",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new(35, 56, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 80,
                Background = MDL.BrushConv("#FFFDE9DF"),
                BorderBrush = MDL.BrushConv("#FF71012B"),
                Height = 24,
                Foreground = MDL.BrushConv("#FF71012B")
            };

            lbox.ItemsSource = (IList)obj;

            buttonB.Click += (object sender, RoutedEventArgs e) =>
            {
                if (lbox.SelectedIndex == -1) return;
                IList ls = (IList)_Dic[nameObj].Obj!;
                ls.RemoveAt(lbox.SelectedIndex);
                lbox.Items.Refresh();
            };

            buttonA.Click += (object sender, RoutedEventArgs e) =>
            {
                object? o = Activator.CreateInstance(obj.GetType().GenericTypeArguments[0]);
                if (o == null) return;
                var wpf = new DialogWindow(o, (cl) =>
                {
                    IList ls = (IList)_Dic[nameObj].Obj!;
                    ls.Add(cl);
                    lbox.Items.Refresh();
                }, _kindTypes);
                wpf.ShowDialog();
            };

            grid.Children.Add(labelA);
            grid.Children.Add(labelB);
            grid.Children.Add(lbox);
            grid.Children.Add(buttonA);
            grid.Children.Add(buttonB);

            MainPanel.Children.Add(grid);

        }
        private void AddOpenClass(Mod m, string nameObj, object obj)
        {
            Grid grid = new()
            {
                Height = 24,
                Background = Brushes.White,
                Width = 800
            };
            Height += 24;
            Label labelA = new()
            {
                Content = m.Comment,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                FontFamily = new("Microsoft Sans Serif"),
                Background = Brushes.FloralWhite,
                Height = 22,
                Padding = new(5, 3, 5, 5),
                Width = 430,
                Uid = nameObj
            };

            Label labelB = new()
            {
                Content = "",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = 14,
                Background = ColorSet(m.MType),
                Margin = new(429, 0, 0, 0),
                Height = 22,
                Width = 10
            };

            Rectangle rectangle = new()
            {
                VerticalAlignment = VerticalAlignment.Top,
                Stroke = MDL.BrushConv("#FF999999"),
                Margin = new(0, 0, 361, 0),
                Height = 22
            };

            Button button = new()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new(439, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 351,
                Height = 22,
                FontFamily = new("Tahoma"),
                FontSize = 13,
                Padding = new(0, 3, 0, 0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Foreground = MDL.BrushConv("#FF000D70"),
                Background = Brushes.White,
                Content = obj is null ? "<Пусто>" : obj.ToString() ?? ""
            };

            void _set(object ob)
            {
                string s = ob.ToString() ?? "";
                button.Content = s;
                _Dic[nameObj] = new DeliveredStruct(s, 0, s, obj!);
            }

            button.Click += (object sender, RoutedEventArgs e) =>
            {
                var wpf = new DialogWindow(obj!, _set, _kindTypes);
                wpf.ShowDialog();
            };

            grid.Children.Add(labelA);
            grid.Children.Add(labelB);
            grid.Children.Add(rectangle);
            grid.Children.Add(button);

            MainPanel.Children.Add(grid);
        }
        private void AddButton(Mod m, string nameObj, Fgis.KindTypes.TypesEnum types, string text)
        {
            const double WP = 176; // размер раскрытого окна
            Grid grid = new()
            {
                Height = 24,
                Background = Brushes.White,
                Width = 800
            };
            Height += 24;
            Label labelA = new()
            {
                Content = m.Comment,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                FontFamily = new("Microsoft Sans Serif"),
                Background = Brushes.FloralWhite,
                Height = 22,
                Padding = new(5, 3, 5, 5),
                Width = 430,
                Uid = nameObj
            };

            Label labelB = new()
            {
                Content = "",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = 14,
                Background = ColorSet(m.MType),
                Margin = new(429, 0, 0, 0),
                Height = 22,
                Width = 10
            };

            Rectangle rectangle = new()
            {
                VerticalAlignment = VerticalAlignment.Top,
                Stroke = MDL.BrushConv("#FF999999"),
                Margin = new(0, 0, 361, 0),
                Height = 22
            };

            Button button = new()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new(439, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 351,
                Height = 22,
                FontFamily = new("Arial"),
                FontSize = 13,
                Padding = new(0, 3, 0, 0),
                BorderBrush = MDL.BrushConv("#FF999999"),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Background = MDL.BrushConv("#FFEDEDED"),
                Content = "<Ничего не выбранно>"
            };

            if (string.IsNullOrEmpty(text) == false)
            {
                button.Content = _kindTypes.GetName(types, text);
            }

            ListBox listBox = new()
            {
                Name = "LBox" + nameObj,
                Background = MDL.BrushConv("#FFF6F6F6"),
                FontFamily = new FontFamily("Calibri"),
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 47, 0, 0)

            };

            Label labelSearch = new()
            {
                Content = "Поиск ",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                FontFamily = new FontFamily("Calibri"),
                Background = Brushes.FloralWhite,
                Height = 19,
                Padding = new Thickness(5, 3, 5, 5),
                Margin = new Thickness(0, 25, 0, 0),
                Width = 72
            };

            TextBox textBoxSearch = new()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                FontFamily = new FontFamily("Calibri"),
                Background = MDL.BrushConv("#FFF0F6E3"),
                Height = 22,
                Padding = new(5, 1, 5, 5),
                Width = 168,
                Margin = new(72, 25, 0, 0),
                IsEnabled = true
            };

            textBoxSearch.TextChanged += (object sender, TextChangedEventArgs e) =>
            {
                string text = ((TextBox)sender).Text;
                if (text.Length > 1)
                {
                    listBox.ItemsSource = _kindTypes.GetDate(types, text);
                }
            };

            button.Click += (object sender, RoutedEventArgs e) =>
            {
                if (listBox.Visibility == Visibility.Visible) return;
                listBox.Visibility = Visibility.Visible;
                grid.Height = 200;
                Height += WP;
                listBox.ItemsSource = _kindTypes.GetDate(types);
            };

            listBox.MouseDoubleClick += (object sender, MouseButtonEventArgs e) =>
            {
                listBox.Visibility = Visibility.Collapsed;
                if (listBox.SelectedItem is DeliveredStruct delivered)
                {
                    button.Tag = delivered;
                    button.Content = delivered.Name;
                    Height -= WP;
                    grid.Height = 24;
                    _Dic[nameObj] = delivered;

                    //Act_abbreviation
                    var m = _cl.GetType().GetMethod("Act_abbreviation");
                    m?.Invoke(_cl, [delivered]);
                }
            };

            grid.Children.Add(labelSearch);
            grid.Children.Add(textBoxSearch);
            grid.Children.Add(labelA);
            grid.Children.Add(labelB);
            grid.Children.Add(rectangle);
            grid.Children.Add(button);
            grid.Children.Add(listBox);

            MainPanel.Children.Add(grid);
        }
        private void AddIntsBox(Mod m, string nameObj, string text)
        {
            Grid grid = new()
            {
                Height = 24,
                Background = Brushes.White,
                Width = 800
            };
            Height += 24;
            Label labelA = new()
            {
                Content = m.Comment,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new("Microsoft Sans Serif"),
                Background = Brushes.FloralWhite,
                Height = 22,
                Padding = new(5, 3, 5, 5),
                Width = 430,
                Uid = nameObj

            };

            Label labelB = new()
            {
                Content = "",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new("Arial"),
                FontSize = 14,
                Background = MDL.BrushConv(m.MType == Mod.MTypeEnum.Required ? Required_Color : Optional_Color),
                Margin = new(429, 0, 0, 0),
                Height = 22,
                Width = 10
            };

            Rectangle rectangle = new()
            {
                Stroke = MDL.BrushConv("#FF999999"),
                Margin = new(0, 0, 361, 0),
                Height = 22
            };

            TextBox textBox = new()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new(439, 0, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 125,
                Height = 22,
                FontFamily = new("Consolas"),
                FontSize = 13,
                Padding = new(0, 3, 0, 0),
                BorderBrush = MDL.BrushConv("#FF999999"),
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            if (m.Max != 0) textBox.MaxLines = m.Max;


            textBox.LostFocus += (object sender, RoutedEventArgs e) =>
            {
                string s = ((TextBox)sender).Text;
                _Dic[nameObj] = new DeliveredStruct(s, 0, s, s);
            };

            grid.Children.Add(labelA);
            grid.Children.Add(labelB);
            grid.Children.Add(rectangle);
            grid.Children.Add(textBox);

            MainPanel.Children.Add(grid);
        }
        private void AddTextBox(Mod m, string nameObj, string text)
        {
            Grid grid = new()
            {
                Height = 24,
                Background = Brushes.White,
                Width = 800
            };
            Height += 24;
            Label labelA = new()
            {
                Content = m.Comment,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new("Microsoft Sans Serif"),
                Background = Brushes.FloralWhite,
                Height = 22,
                Padding = new(5, 3, 5, 5),
                Width = 430,
                Uid = nameObj
            };

            Label labelB = new()
            {
                Content = "",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new("Arial"),
                FontSize = 14,
                Background = MDL.BrushConv(m.MType == Mod.MTypeEnum.Required ? Required_Color : Optional_Color),
                Margin = new(429, 0, 0, 0),
                Height = 22,
                Width = 10
            };

            Rectangle rectangle = new()
            {
                Stroke = MDL.BrushConv("#FF999999"),
                Margin = new(0, 0, 361, 0),
                Height = 22
            };

            TextBox textBox = new()
            {
                Tag = m,
                Name = nameObj,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new(439, 0, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 351,
                Height = 22,
                FontFamily = new("Microsoft Sans Serif"),
                FontSize = 13,
                Padding = new(0, 3, 0, 0),
                BorderBrush = MDL.BrushConv("#FF999999")
            };
            if (m.Max != 0) textBox.MaxLines = m.Max;

            textBox.LostFocus += (object sender, RoutedEventArgs e) =>
            {
                string s = ((TextBox)sender).Text;
                _Dic[nameObj] = new DeliveredStruct(s, 0, s, s);
            };

            grid.Children.Add(labelA);
            grid.Children.Add(labelB);
            grid.Children.Add(rectangle);
            grid.Children.Add(textBox);

            MainPanel.Children.Add(grid);
        }
        private void AddDateBox(Mod m, string nameObj, string text)
        {
            Grid grid = new()
            {
                Height = 24,
                Background = Brushes.White,
                Width = 800
            };
            Height += 24;
            Label labelA = new()
            {
                Content = m.Comment,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new("Microsoft Sans Serif"),
                Background = Brushes.FloralWhite,
                Height = 22,
                Padding = new(5, 3, 5, 5),
                Width = 430,
                Uid = nameObj
            };

            Label labelB = new()
            {
                Content = "",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new("Arial"),
                FontSize = 14,
                Background = MDL.BrushConv(m.MType == Mod.MTypeEnum.Required ? Required_Color : Optional_Color),
                Margin = new(429, 0, 0, 0),
                Height = 22,
                Width = 10
            };

            Rectangle rectangle = new()
            {
                Stroke = MDL.BrushConv("#FF999999"),
                Margin = new(0, 0, 361, 0),
                Height = 22
            };

            DatePicker picker = new()
            {
                Tag = m,
                Name = nameObj,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new(439, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Width = 120,
                Height = 22,
                FontFamily = new("Microsoft Sans Serif"),
                FontSize = 13,
                Padding = new(0, 3, 0, 0),
                BorderBrush = MDL.BrushConv("#FF999999")
            };

            if (DateTime.TryParse(text, out DateTime date)) picker.SelectedDate = date;

            picker.LostFocus += (object sender, RoutedEventArgs e) =>
            {
                DateTime? t = ((DatePicker)picker).SelectedDate ?? DateTime.Now;
                var s = t.Value.ToString("yyyy-MM-dd");
                if (t != null) _Dic[nameObj] = new DeliveredStruct(s, 0, s, s);
            };

            grid.Children.Add(labelA);
            grid.Children.Add(labelB);
            grid.Children.Add(rectangle);
            grid.Children.Add(picker);

            MainPanel.Children.Add(grid);
        }

        private void Закрыть_документ(object sender, RoutedEventArgs e) => Close();

        private void ПринятьИзменения(object sender, RoutedEventArgs e)
        {
            if (FullClass(_cl))
            {
                _action(_cl);
                Закрыть_документ(null!, null!);
            }
        }
    }
}
