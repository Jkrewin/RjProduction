using RjProduction.Model;
using RjProduction.Sql;
using RjProduction.XML;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace RjProduction.Pages.Doc
{
    
    public partial class PageShipments : Page
    {
        private readonly IDocMain _Doc;
        private bool RenameGrup = false;    //Режим переименование группы
        private bool SavedDoc = false;      //Документ пока не сохранялся
        private readonly Action ClosePage;  //Ссылка на метод закрытия это формы
        private readonly UIElement? MainFramePanel;

        public PageShipments(DocShipments doc, UIElement framePanel, Action closePage)
        {
            InitializeComponent();
            _Doc = doc;
            ClosePage = closePage;
            MainFramePanel = framePanel;
            Title = doc.Number + "/" + doc.DataCreate.ToString();
            WarehouseSelector.Visibility = Visibility.Collapsed;
        }

        public PageShipments(DocMoving doc, UIElement framePanel, Action closePage)
        {
            InitializeComponent();
            _Doc = doc;
            ClosePage = closePage;
            MainFramePanel = framePanel;
            Title = doc.Number + "/" + doc.DataCreate.ToString();
            WarehouseSelector.Visibility = Visibility.Visible;
            Cbox_warehouses_From.Content = doc.Warehouse_From.NameWarehouse;
            Cbox_warehouses_To.DisplayMemberPath = "NameWarehouse";
            Cbox_warehouses_To.ItemsSource = MDL.MyDataBase.Warehouses;
        }

        private void Refreh_ListGrup() {
            ListGrup.Items.Clear();
            ListBoxEmp.Items.Clear();
            
            foreach (var item in _Doc.MainTabel)
            {
                ListGrup.Items.Add(item.NameGrup);
            }
        }

        private void Refreh_ListBoxEmp()
        {
            if (ListGrup.SelectedIndex == -1) return;
            ListBoxEmp.Items.Clear();

            foreach (var item in _Doc.MainTabel[ListGrup.SelectedIndex].Tabels)
            {
                if (item is Pseudonym products) AddItemEmp(products);
            }
            Label_SumDown.Content = _Doc.MainTabel[ListGrup.SelectedIndex].Tabels.Sum(x => x.Amount);
        }

        private void AddItemEmp(Pseudonym obj) {
            string cub = (obj.Operation == SqlRequest.OperatonEnum.vsMunis ? "-" : "+") + Math.Round(obj.CubatureAll,2).ToString();
            /// добавление новых элементов изменить Text_TextChanged структуру
            StackPanel sp = new() { Orientation = Orientation.Horizontal, Height = 20 };
            Label l1 = new()
            {
                Width = 250,
                Padding = new(5, 1, 5, 5),
                Content = obj.Product.NameItem + " !"+ Math.Round( obj.CubatureAll,2)
            };
            if (obj.Product.SyncError) l1.Foreground = Brushes.Red;
            sp.Children.Add(l1);
            Label l0 = new()
            {
                Width = 50,
                Padding = new(5, 1, 5, 5),
                Foreground = MDL.BrushConv(obj.Operation == SqlRequest.OperatonEnum.vsMunis ? "#FFD39B9B" : "#FF029E88"),
                Content = (obj.Operation == SqlRequest.OperatonEnum.vsMunis ? "-" : "+") + Math.Round(obj.SelectedCub, 2)
            };
            sp.Children.Add(l0);
            TextBox text1 = new()
            {
                Width = 80,
                Text = obj.PriceCng.ToString(),
                BorderBrush = null,
                Background = MDL.BrushConv("#FFD2D9F3"),
                Tag = sp
            };
            text1.TextChanged += Text_TextChanged;
            text1.GotFocus += Text_GotFocus;
            text1.LostFocus += Text_LostFocus;
            sp.Children.Add(text1);
            Label l2 = new()
            {
                Padding = new(5, 1, 5, 5),
                Content = obj.Amount
            };
            sp.Tag = obj;
            sp.Children.Add(l2);
            ListBoxEmp.Items.Add(sp);
        }

        private void Text_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox text_box)
            {
                var tag = (StackPanel)text_box.Tag;
                if (tag.Tag is Pseudonym pseudonym) ((TextBox)sender).Text = pseudonym.PriceCng.ToString();
            }
        }

        private void Text_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox text_box) text_box.Text = "";
        }

        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            const int LABEL_INT_AMOUNT = 3;
            if (sender is TextBox text_box)
            {
                var tag = (StackPanel)text_box.Tag;
                var label = (tag.Children[LABEL_INT_AMOUNT] as Label) ?? new Label();
                if (tag.Tag is Pseudonym pseudonym)
                {
                    if (double.TryParse(((TextBox)sender).Text, out double d)) pseudonym.PriceCng = d;
                    label.Content = pseudonym.Amount;
                }
            }
            Label_SumDown.Content = _Doc.MainTabel[ListGrup.SelectedIndex].Tabels.Sum(x => x.Amount);
        }

        private void Загруженно(object sender, System.Windows.RoutedEventArgs e)
        {
            MDL.ImportToWpf(this, _Doc);
            Refreh_ListGrup();
        }

        private void ВыходИзДаты_(object sender, System.Windows.RoutedEventArgs e)
        {
            //=DateOnly.FromDateTime(((DatePicker)sender).SelectedDate!.Value)
        }

        private void ВодНомера(object sender, System.Windows.RoutedEventArgs e)
        {
            if (uint.TryParse(((TextBox)sender).Text, out uint u)) _Doc.Number = u;
            else ((TextBox)sender).Text = _Doc.Number.ToString();
        }

        private void ВходВПоле_(object sender, System.Windows.RoutedEventArgs e) => ((TextBox)sender).Text = "";

        private void ДобавитьОбъект(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void ДобавитьПМат(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void ДобавитьСотрудника(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void ДобавитьКруглыйЛес(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void ДобавитьДоплата(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void НажитиеЗакрыть(object sender, System.Windows.RoutedEventArgs e)
        {
            if (SavedDoc == false)
            {
                var b = MessageBox.Show("Вы не сохранили документ. Закрыть этот документ без сохранения ? ", "Изменения в документ", MessageBoxButton.YesNo);
                if (b == MessageBoxResult.No) return;
            }
                       
            ClosePage?.Invoke();           
        }

        private void ЗакрытьОкноГруппы(object sender, System.Windows.RoutedEventArgs e)
        {
             Grid_NameGrup.Visibility = Visibility.Collapsed;
        }

        private void ДобавитьГруппуСписок(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TBox_GrupName.Text)) return;
            if (_Doc.MainTabel.Any(x => x.NameGrup == TBox_GrupName.Text))
            {
                MessageBox.Show("Группа с таким названием уже существует ");
                return;
            }

            // обновляет список MDL если нет значения в списке
            if (!MDL.MyDataBase.NamesGrup.Any(x => x == TBox_GrupName.Text))
            {
                MDL.MyDataBase.NamesGrup.Add(TBox_GrupName.Text);
                TBox_GrupName.Items.Refresh();
            }

            if (RenameGrup == true & ListGrup.SelectedIndex != -1)
            {
                _Doc.MainTabel [ListGrup.SelectedIndex].NameGrup = TBox_GrupName.Text;
                RenameGrup = false;
            }
            else _Doc.MainTabel.Add(new GrupObj() { NameGrup = TBox_GrupName.Text });
            //_Shipments.MainTabel[ListGrup.SelectedIndex] = _Shipments.MainTabel[^1];
            Refreh_ListGrup();
            ЗакрытьОкноГруппы(null!, null!);
        }

        private void КлавишаДобовитьГруппу(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ДобавитьГруппуСписок(null!, null!);
        }

        private void УдалитьОбъект(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ListGrup.SelectedIndex == -1)
            {
                MessageBox.Show("Сначало нужно выбрать группу");
                return;
            }
            if (ListBoxEmp.SelectedIndex == -1)
            {
                _Doc.MainTabel[ListGrup.SelectedIndex].Tabels.RemoveAt(ListBoxEmp.SelectedIndex);
                Refreh_ListBoxEmp();
            }
            else
            {
                MessageBox.Show("Сначало нужно выбрать объект из списка ниже");
            }
        }

        private void РедактированиеОбъекта(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        [DependentCode]
        private void СохранитьXML(object sender, System.Windows.RoutedEventArgs e)
        {
            if (SavedDoc == false)
            {
                string sFile = $"{MDL.XmlPatch(_Doc.DataCreate)}\\{((XmlProtocol)_Doc).FileName(_Doc.Doc_Code)}";
                if (File.Exists(sFile))
                {
                    if (MessageBox.Show("Перезаписать ранее созданный файл с такой датой и номером ?", "", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
                }
            }
            if (uint.TryParse(Number.Text, out uint u)) MDL.MyDataBase.NumberDef = u;
            if (_Doc is DocShipments) XmlProtocol.SaveDocXml<DocShipments>(_Doc);
            else if (_Doc is DocMoving) XmlProtocol.SaveDocXml<DocMoving>(_Doc);
            else if (_Doc is DocWritedowns) XmlProtocol.SaveDocXml<DocWritedowns>(_Doc);
            SavedDoc = true;
        }

        private void ДобавитьГруппу(object sender, System.Windows.RoutedEventArgs e)
        {
            Grid_NameGrup.Visibility = Visibility.Visible;
            TBox_GrupName.Focus();
        }

        private void УдалитьГруппу(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ListGrup.SelectedIndex == -1) return;
            _Doc.MainTabel.RemoveAt(ListGrup.SelectedIndex);
            Refreh_ListGrup();
        }

        private void ВыбраннаГруппа(object sender, SelectionChangedEventArgs e)
        {
            Refreh_ListBoxEmp();
        }

        private void ПереименоватьГруппу(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ListGrup.SelectedIndex == -1) return;
            Grid_NameGrup.Visibility = Visibility.Visible;
            TBox_GrupName.Text = _Doc.MainTabel[ListGrup.SelectedIndex].NameGrup;
            TBox_GrupName.Focus();
            RenameGrup = true;
        }

        private void ВыполнитьЛокумент(object sender, RoutedEventArgs e)
        {
            if (_Doc.Status == StatusEnum.Проведен) {
                MessageBox.Show("Документ был уже ранее проведен");
                return;
            }

            if (SavedDoc == false)
            {
                MessageBox.Show("Сначало нужно сохранить документ. Чтобы его провести ");
            }
            else
            {
                _Doc.CarryOut();
            }
        }

        private void РазвернутьЭкран(object sender, RoutedEventArgs e)
        {
            CloseWpf.Visibility = Visibility.Collapsed;
            ((Button)sender).Visibility = Visibility.Collapsed;
            if (MainFramePanel is not null) MainFramePanel.Visibility = Visibility.Collapsed;
            var wpf = new WpfFrm.WpfView(this,Title ,ClosePage);
            wpf.Show();
        }

        private void СоздатьСкладB(object sender, RoutedEventArgs e)
        {
            WpfFrm.WpfWarehouse wpf = new(
                (w) =>
                {
                    Cbox_warehouses_To.Items.Refresh();
                    if (w is null) return;
                    for (global::System.Int32 i = 0; i < MDL.MyDataBase.Warehouses.Count; i++)
                    {
                        if (w.Equals(MDL.MyDataBase.Warehouses[i]))
                        {
                            Cbox_warehouses_To.SelectedIndex = i;
                            break;
                        }
                    }
                });
            wpf.ShowDialog();
        }
        
        private void ВыбранСкладTo(object sender, SelectionChangedEventArgs e)
        {
            var w = MDL.MyDataBase.Warehouses.Find(x => x.Equals(Cbox_warehouses_To.SelectedValue)) ; 
            ((DocMoving)_Doc).Warehouse_To = w ?? new WarehouseClass() { NameWarehouse = "NaN" };
        }
    }
}
