
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RjProduction.Model.DocElement;

namespace RjProduction.Pages
{
    public partial class PageTimbers : Page, IKeyControl
    {
        double _LongWood = 0;

        private double LongWood { get => _LongWood; 
            set 
            {
                if (LSCubs.Any(x => x.Long == value)) {
                    _LongWood=value;
                    LongWoods.Text = value.ToString();
                }
                else LongWoods.Text = value.ToString();
            } 
        }
        private Tabel_Timbers.Timber? TmpTimber;
        private Tabel_Timbers.Timber? DelTimber;
        private Action? InputBoxAction;
        private Tabel_Timbers.Timber? EndEdit;
        private readonly Tabel_Timbers _TabelTimbers;
        private readonly Action<Tabel_Timbers> ActionOne;
        private readonly Action CloseAction;
        private List<Model.Classifier.RoundTimberCub> LSCubs = [];

        public PageTimbers(Tabel_Timbers tabelTimbers, Action<Tabel_Timbers> actionOne, Action closeAction)
        {
            InitializeComponent();
            _TabelTimbers = tabelTimbers;
            ActionOne = actionOne;
            CloseAction = closeAction;          
        }

      
        /// <summary>
        /// Обновляет таблицу куботуры
        /// </summary>
        private void Refreh_DG_Cubs()
        {     
                try
                {
                    DG_Cubs.Items.Refresh();
                    LabelItog.Content = $"Количество: {_TabelTimbers.Timbers.Sum(x => x.Количество)} Куб/м: {Math.Round(_TabelTimbers.Timbers.Sum(x => x.Куб_М), 3)}";
                }
                catch
                {
                    LabelItog.Content = $"Количество: потвердить Enter";
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
        /// <summary>
        /// Находит и расчитывает кубы на 1 бревно
        /// </summary>
        /// <param name="diametor"></param>
        /// <returns></returns>
        private double FindCubs(int diametor) {
            var cubs = LSCubs.Find(x => x.Long == LongWood); // получем список всех диаметров
            double dd = cubs.DictionarySize(diametor.ToString());
            // подборка похожего диаметра для отсутствующего диатмера
            if (dd == -1)     //измен dd % 2 != 0 & dd==0
            {
                var d1 = cubs.DictionarySize((diametor - 1).ToString());
                var d2 = cubs.DictionarySize((diametor + 1).ToString());
                if (d1 == 0 | d2 == 0)
                {
                    MessageBox.Show("Справочник не содержит такого диаметра " + diametor);
                    return -1;
                }
                dd = (d1 + d2) / 2; // формула среднего куба
            }
            return dd;
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {           
            LSCubs = Model.Classifier.RoundTimberCub.LoadList(MDL.Dir_Resources + "RoundTimberCub.xml");

            InputTextBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) Yes_Click(null!, null!); };


            Style cellStyle = new (typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, MDL.BrushConv("#FFF6E8E8")));
            cellStyle.Setters.Add(new Setter(DataGridCell.FontWeightProperty, FontWeights.Normal));


            DG_Cubs.ItemsSource = _TabelTimbers.Timbers;
            DG_Cubs.Columns[0].IsReadOnly = true;
            DG_Cubs.Columns[0].CellStyle = cellStyle;
            DG_Cubs.Columns[3].IsReadOnly = true;
            DG_Cubs.Columns[3].CellStyle = cellStyle;
            DG_Cubs.Columns[5].IsReadOnly = true;
            DG_Cubs.Columns[5].CellStyle = cellStyle;
            DG_Cubs.Columns[6].Header = "Доплата";

            DG_Cubs.CellEditEnding += ИзмененияВнесены;
            DG_Cubs.CurrentCellChanged += ЯчекаИзменена;
            var window = Window.GetWindow(this);
          // window.KeyDown += HandleKeyPress;

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

        private void ЗакрытьФорму(object sender, RoutedEventArgs e) { 
            CloseAction?.Invoke();
        }

        private void УдалитьЗначение(object sender, RoutedEventArgs e)
        {
            if (DelTimber != null)
            {
                _TabelTimbers.Timbers.Remove((Tabel_Timbers.Timber)DelTimber);
                Refreh_DG_Cubs();
            }
        }

        private void ВыборОбъекта(object sender, SelectedCellsChangedEventArgs e)
        {
            if (e.AddedCells.Count != 0) if (e.AddedCells[0].Item is Tabel_Timbers.Timber cl) DelTimber = cl;
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
                        TmpTimber.Количество += d;
                        var dd = FindCubs(TmpTimber.Диаметр);
                        TmpTimber.Куб_М = dd * TmpTimber.Количество;
                        // добавить если нет в списке 
                        if (_TabelTimbers.Timbers.Any(x=>x.Диаметр== TmpTimber.Диаметр & x.Длинна==LongWood)==false) _TabelTimbers.Timbers.Add(TmpTimber);
                        Refreh_DG_Cubs();
                    }
                    else MessageBox.Show("Неверно указано количество.");
                });
            };

            void actII()
            {
                InpubBoxOpen("Введите Диаметр бревна",
                  () =>
             {
               InputTextBox.Text = InputTextBox.Text.Trim();
               if (int.TryParse(InputTextBox.Text, out int d))
               {                    
                     var f = _TabelTimbers.Timbers.Find(x => x.Диаметр == d & x.Длинна == LongWood);
                     if (f != null)
                     {
                         // нужно добавить в общему количеству бревен 
                         TmpTimber = f;
                     }
                     else {
                         TmpTimber.Диаметр = d;
                     }
                    act();
               }
               else MessageBox.Show("Справочник не содержит такого диаметра");
              });
            }

            if (LongWood == 0) // Если ранее на был введен длинна бревна
            {
                InpubBoxOpen("Введите Длинну бревна", () =>
                {
                    InputTextBox.Text = InputTextBox.Text.Replace('.', ',');
                    if (double.TryParse(InputTextBox.Text, out double d))
                    {
                        if (LSCubs.Any(x => x.Long == d))
                        {
                            LongWood = d;
                            TmpTimber.Длинна = d;
                            actII();
                        }
                        else { MessageBox.Show("Справочник не содержит такой длинны"); }
                    }
                });
            }
            else actII();
        }

        private void ЯчекаИзменена(object? sender, EventArgs e)
        {
            if (EndEdit != null)
            {               
                if (LSCubs.Any(x => x.Long == EndEdit.Длинна) == false)
                {
                    MessageBox.Show("Справочник не содержит такой длинны");
                    goto end;
                }               
                var s = FindCubs(EndEdit.Диаметр);
                if (s == 0)
                {
                    MessageBox.Show("Справочник не содержит такого диаметра");
                }
                else
                {
                    LongWood = EndEdit.Длинна;
                    EndEdit.Куб_М = EndEdit.Количество * s;
                }
            end:;
                EndEdit = null;
                Refreh_DG_Cubs();
            }
        }

        private void ИзмененияВнесены(object? sender, DataGridCellEditEndingEventArgs e)
        {           
            if (e.EditAction == DataGridEditAction.Commit)
            {
                EndEdit = e.Row.Item as Tabel_Timbers.Timber;                
            }
        }

        void IKeyControl.HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1) Кубатурник_ОК(null!, null!);
            else if (e.Key == Key.Escape) ЗакрытьФорму(null!, null!);
            else if (e.Key == Key.Add ) НовыйЭлемент(null!, null!);
            else if (e.Key == Key.F3) УдалитьЗначение(null!, null!);
        }

        private void ЦенаДляВсех(object sender, RoutedEventArgs e)
        {
            foreach (var item in _TabelTimbers.Timbers)
            {
                if (double.TryParse ( TBox_AllPrice.Text, out double d )){ 
                    item.Цена = d;                     
                }                
            }
            Refreh_DG_Cubs();
        }

        private void ИзменитьДлинну(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(LongWoods.Text, out double d))
            {
                LongWood = d;
            }
            else
            {
                LongWoods.Text = LongWood.ToString();
            }
        }

        private void ИзменениеТекстаПоле(object sender, TextChangedEventArgs e)
        {
            if (InputTextBox.Text =="+") InputTextBox.Text = "";
        }

        private void НажатиеКлавишТаблице(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DG_Cubs.CommitEdit(DataGridEditingUnit.Row, true);
            }
        }
    }
}
