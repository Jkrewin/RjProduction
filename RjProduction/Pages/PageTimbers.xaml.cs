﻿using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RjProduction.Model;

namespace RjProduction.Pages
{
    public partial class PageTimbers : Page
    {
        private double LongWood = 0;
        private Timber? TmpTimber;
        private Timber? DelTimber;
        private readonly List<CubGude> DicCubs = [];
        private Action? InputBoxAction;
        private Timber? EndEdit;
        private readonly Tabel_Timbers _TabelTimbers;
        private readonly Action<Tabel_Timbers> ActionOne;
        private readonly Action CloseAction;

        public PageTimbers(Tabel_Timbers tabelTimbers, Action<Tabel_Timbers> actionOne, Action closeAction)
        {
            InitializeComponent();
            _TabelTimbers = tabelTimbers;
            ActionOne = actionOne;
            CloseAction = closeAction;

        }

        /// <summary>
        /// Справочник структура строк
        /// </summary>
        private readonly struct CubGude
        {
            public readonly int Diameter;
            public readonly Dictionary<double, double> Sizes;

            public CubGude(int diameter, double[] sizes)
            {
                Diameter = diameter;
                double d = 3.5;
                Sizes = [];

                foreach (var item in sizes)
                {
                    Sizes.Add(d, item);
                    d += 0.5;
                }
            }
        }
        /// <summary>
        /// Обновляет таблицу куботуры
        /// </summary>
        private void Refreh_DG_Cubs()
        {
            if (EndEdit == null)
            {
                LabelItog.Content = $"Количество: {_TabelTimbers.Timbers.Sum(x => x.Количество)} Куб/м: {Math.Round(_TabelTimbers.Timbers.Sum(x => x.Куб_М), 3)}";
                DG_Cubs.Items.Refresh();
            }
        }
        /// <summary>
        /// Окно ввода аналог inputBox
        /// </summary>
        /// <param name="title">по умолчанию занчение</param>
        /// <param name="action">действие</param>
        private void InpubBoxOpen(string title, Action action)
        {
            InputBox.Visibility = Visibility.Visible;
            InputBox_TextBlock.Text = title;
            InputTextBox.Focus();
            InputBoxAction = action;
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            string s = AppDomain.CurrentDomain.BaseDirectory + "Res\\Кубатурники\\GOST2078-75B.txt";
            if (File.Exists(s))
            {
                var t = File.ReadAllLines(s);
                for (int i = 1; i < t.Length; i++)
                {
                    var arr = t[i].Split('\t');
                    int start = int.Parse(arr[0]);
                    List<double> queue = [];
                    for (int j = 1; j < arr.Length; j++) queue.Add(double.Parse(arr[j]));
                    DicCubs.Add(new CubGude(start, [.. queue]));
                }
            }
            else MessageBox.Show("Кубатурник не был найден +" + s);

            InputTextBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) Yes_Click(null!, null!); };

            DG_Cubs.ItemsSource = _TabelTimbers.Timbers;
            DG_Cubs.Columns[0].IsReadOnly = true;
            DG_Cubs.Columns[5].IsReadOnly = true;

            DG_Cubs.CellEditEnding += ИзмененияВнесены;
            DG_Cubs.CurrentCellChanged += ЯчекаИзменена;

            PreviewKeyUp += (object sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.F1) Кубатурник_ОК(null!, null!);
                else if (e.Key == Key.Escape) CloseAction?.Invoke();
            };

            Refreh_DG_Cubs();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = Visibility.Collapsed;
            InputTextBox.Text = string.Empty;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = Visibility.Collapsed;
            if (!string.IsNullOrEmpty(InputTextBox.Text)) InputBoxAction?.Invoke();
            InputTextBox.Text = string.Empty;
        }

        private void Кубатурник_ОК(object sender, RoutedEventArgs e)
        {
            ActionOne?.Invoke(_TabelTimbers);
            ЗакрытьФорму(null!, null!);
        }

        private void ЗакрытьФорму(object sender, RoutedEventArgs e) => CloseAction?.Invoke();

        private void УдалитьЗначение(object sender, RoutedEventArgs e)
        {
            if (DelTimber != null)
            {
                _TabelTimbers.Timbers.Remove((Timber)DelTimber);
                Refreh_DG_Cubs();
            }
        }

        private void ВыборОбъекта(object sender, SelectedCellsChangedEventArgs e)
        {
            if (e.AddedCells.Count != 0) if (e.AddedCells[0].Item is Timber cl) DelTimber = cl;
        }

        private void НовыйЭлемент(object sender, RoutedEventArgs e)
        {
            TmpTimber = new() { Длинна = LongWood };

            void act()
            {
                InpubBoxOpen("Введите Колличество бревен", () =>
                {
                    if (int.TryParse(InputTextBox.Text, out int d))
                    {
                        TmpTimber.Количество = d;
                        double dd = DicCubs.Find(x => x.Diameter == TmpTimber.Диаметр).Sizes[LongWood];
                        TmpTimber.Куб_М = dd * d;
                        _TabelTimbers.Timbers.Add(TmpTimber);
                        Refreh_DG_Cubs();
                    }
                    else MessageBox.Show("Неверно указано количество.");
                });
            };


            InpubBoxOpen("Введите Диаметр бревна",
            () =>
            {
                if (int.TryParse(InputTextBox.Text, out int d))
                {
                    if (_TabelTimbers.Timbers.Find(x => x.Диаметр == d) != null)
                    {
                        MessageBox.Show("Такой диаметор бревна уже в списке");
                        return;
                    }
                    if (DicCubs.Any(x => x.Diameter == d))
                    {
                        TmpTimber.Диаметр = d;
                        // Если ранее на был введен длинна бревна
                        if (LongWood == 0)
                        {
                            InpubBoxOpen("Введите Длинну бревна", () =>
                            {
                                if (double.TryParse(InputTextBox.Text, out double d))
                                {
                                    if (DicCubs.Find(x => x.Diameter == TmpTimber.Диаметр).Sizes.ContainsKey(d))
                                    {
                                        LongWood = d;
                                        TmpTimber.Длинна = d;
                                        act();
                                    }
                                    else MessageBox.Show("Справочник не содержит такой длинны");
                                }
                            });
                        }
                        // Запрос количества если ранее был введено длинна бревна
                        else act();
                    }
                    else MessageBox.Show("Справочник не содержит такого диаметра");
                }
            });
        }

        private void ЯчекаИзменена(object? sender, EventArgs e)
        {
            if (EndEdit != null)
            {
                var s = DicCubs.Find(x => x.Diameter == EndEdit.Диаметр);
                if (s.Diameter != 0)
                {
                    if (s.Sizes.TryGetValue(EndEdit.Длинна, out double value))
                    {
                        LongWood = EndEdit.Длинна;
                        EndEdit.Куб_М = EndEdit.Количество * value;
                    }
                }
                EndEdit = null;
                Refreh_DG_Cubs();
            }
        }

        private void ИзмененияВнесены(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit) EndEdit = (Timber?)e.Row.Item;
        }
    }
}
