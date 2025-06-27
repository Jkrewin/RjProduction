
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RjProduction.Model.Catalog;
using System.Collections.ObjectModel;

namespace RjProduction.Pages
{
    /// <summary>
    /// Открывает список элементов для выбора. Не универсален требует измнения  OpenWpfDialog
    /// 
    /// </summary>
    public class ToolList<T> where T : IToolFind
    {
        private readonly MDL.Reference.Catalog<T>  Catalog;
        private readonly ObservableCollection<T> CollectionView;
        private readonly Grid ControlGrid;
        private Grid? WinList;
        private object? SelectedObj;
        private readonly Action<object> Act;
        private readonly int IndexRow;
        private bool FirstEnter = false;
        private readonly Action<Action<object>> OpenWpfDialog;

        /// <summary>
        /// Список для выбора элементов
        /// </summary>
        /// <param name="grid">Тот где будет распологаться список</param>
        /// <param name="list">Данный в список</param>
        /// <param name="dialog">Диалог меню при создание и изменение из списка</param>
        /// <param name="act">действие при выборе</param>
        /// <param name="indexRow">индекс в grid Row где появиться список по умолчанию 1</param>
        public ToolList(Grid grid, Action<object> act, int indexRow = 1)
        {
            ControlGrid = grid;
            IndexRow = indexRow;            
            Act = act;
            switch (typeof(T).Name)
            {
                case nameof(Model.Catalog.Company):
                    Catalog = new MDL.Reference.Catalog<T>();
                    CollectionView = [.. Catalog.ListCatalog.Cast<T>().ToList()];
                    OpenWpfDialog = (actII) =>
                    {
                        WpfFrm.WPF_Company a = new((Company)(SelectedObj ?? new Company()), actII);
                        a.ShowDialog();
                    };
                    break;
                default:
                    throw new ArgumentException("Отсутствует DialogEnum элемент");
            }
            Load();
        }

        private void Load()
        {
            WinList = new()
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x33, 0, 0, 0))
            };

            StackPanel stackPanel = new()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            ListBox listBox = new()
            {
                Width = 350,
                MinHeight = 150,
                MaxHeight = 500,
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x75, 0x60, 0x00)),
                Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x17, 0x5C))
            };

            StackPanel stackPanel_upp = new()
            {
                Orientation = Orientation.Horizontal,
                Height = 32,
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF6, 0xCA, 0x00)) // Желтый фон
            };

            Button button1 = CreateButton("\uE160", "Добавить", "#FFC69E06", "#FF806200");
            Button button2 = CreateButton("\uE1C2", "Изменить", "#FFC69E06");
            Button button3 = CreateButton("\uE107", "Удалить", "#FFC69E06");
            Button button_exit = CreateButton("", "Закрыть меню", "#FFC69E06");

            TextBox textBox = new()
            {
                TextWrapping = TextWrapping.NoWrap,
                Text = "поиск",
                Width = 200,
                Height = 18,
                Margin = new Thickness(10, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xE1, 0xDE, 0xAF)),
                Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x86, 0x47, 0x00)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x4F, 0x4F, 0x4F))
            };

            listBox.ItemsSource = CollectionView;
            listBox.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                SelectedObj = ((ListBox)sender).SelectedValue;
            };

            textBox.GotFocus += (object sender, RoutedEventArgs e) =>
            {
                if (!FirstEnter)
                {
                    FirstEnter = true;
                    textBox.Text = "";
                }
            };

            button_exit.Click += Button_exit_Click;

            listBox.MouseDoubleClick += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
            {
                if (SelectedObj != null) Act(SelectedObj);
                Button_exit_Click(null!, null!);
            };

            textBox.TextChanged += (object sender, TextChangedEventArgs e) =>
            {
                // тут поиск
                if (textBox.Text.Length > 2)
                {
                    CollectionView.Clear();
                    foreach (var item in Catalog.ListCatalog)
                    {
                        if (item.Finder.Contains(textBox.Text, StringComparison.OrdinalIgnoreCase)) CollectionView.Add(item);
                    }
                }
                else if (Catalog.ListCatalog.Count != CollectionView.Count) 
                {
                    foreach (var item in Catalog.ListCatalog) CollectionView.Add(item);
                }
            };
            // Новая строка открыть окно 
            button1.Click += (object sender, RoutedEventArgs e) =>
            {
                SelectedObj = null;
                OpenWpfDialog((s) =>
                    {
                        CollectionView.Add((T)s);
                        Catalog.ListCatalog.Add((T)s);
                    });
            };
            // Изменить строку окрыть окно
            button2.Click += (object sender, RoutedEventArgs e) =>
            {
                if (SelectedObj != null)
                {
                    OpenWpfDialog((s) =>
                    {
                        SelectedObj = s;
                        listBox.Items.Refresh();
                    });
                }
            };
            // Удалить строку
            button3.Click += (object sender, RoutedEventArgs e) =>
            {
                if (SelectedObj != null)
                {
                    CollectionView.Remove((T)SelectedObj);
                    Catalog.ListCatalog.Remove((T)SelectedObj);
                }
            };

            stackPanel_upp.Children.Add(button1);
            stackPanel_upp.Children.Add(button2);
            stackPanel_upp.Children.Add(button3);
            stackPanel_upp.Children.Add(button_exit);
            stackPanel_upp.Children.Add(textBox);

            stackPanel.Children.Add(stackPanel_upp);
            stackPanel.Children.Add(listBox);

            WinList.Children.Add(stackPanel);
            ControlGrid.Children.Add(WinList);

            Grid.SetRow(WinList, IndexRow);
            button1.Focus();
        }

        private void Button_exit_Click(object sender, RoutedEventArgs e)
        {
            ControlGrid.Children.Remove(WinList);
            Catalog.SaveData();
        }

        private static Button CreateButton(string content, string helpText, string backgroundColor, string? borderBrushColor = null)
        {
            // Создать кнопку по шаблону
            Button button = new()
            {
                Content = content,
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Width = 24,
                Height = 24,
                ToolTip = helpText,
                Margin = new Thickness(10, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(0x33,
                    byte.Parse(backgroundColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(backgroundColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(backgroundColor.Substring(7, 2), System.Globalization.NumberStyles.HexNumber))),
                BorderBrush = borderBrushColor != null ? new SolidColorBrush(Color.FromArgb(
                    0xFF,
                    byte.Parse(borderBrushColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(borderBrushColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(borderBrushColor.Substring(7, 2), System.Globalization.NumberStyles.HexNumber))) : null
            };

            // Добавляем стиль для скругления углов
            Style borderStyle = new(typeof(Border));
            borderStyle.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(3)));
            button.Resources.Add(typeof(Border), borderStyle);

            return button;
        }     
    }
}
