using RjProduction.Model;
using RjProduction.Sql;
using RjProduction.XML;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RjProduction.Model.DocElement;
using static RjProduction.XML.BlankDoc.Invoice;
using RjProduction.XML.BlankDoc;
using RjProduction.Model.Catalog;
using RjProduction.Pages.Editors;

namespace RjProduction.Pages.Doc
{
    
    public partial class PageShipments : Page
    {
       
        private readonly IDocMain _Doc;       
        private bool SavedDoc = false;      //Документ пока не сохранялся
        private readonly Action ClosePage;  //Ссылка на метод закрытия это формы
        private Action? RenameFoo;

        private bool СоСкладаНаСклад
        {
            get => TabWarehouseSelector.Visibility == Visibility.Visible; set
            {
                if (value) TabWarehouseSelector.Visibility = Visibility.Visible;
                else TabWarehouseSelector.Visibility = Visibility.Collapsed;
            }
        }

        private bool ЕстьПокупатель
        {
            get => TabGrid_Buyer.Visibility == Visibility.Visible; set
            {
                if (value) TabGrid_Buyer.Visibility = Visibility.Visible;
                else TabGrid_Buyer.Visibility = Visibility.Collapsed;
            }
        }

        private bool УказатьПричинуСписания
        {
            get => Grid_Writedowns.Visibility == Visibility.Visible; set
            {
                if (value) Grid_Writedowns.Visibility = Visibility.Visible;
                else Grid_Writedowns.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Выбранная группа
        /// </summary>
        private GrupObj? SelectGrup { get => _Doc.MainTabel.Find(x=> x.NameGrup == ListGrup.SelectedValue.ToString()); }

        private void Clear_UI() { 
            СоСкладаНаСклад = false;
            ЕстьПокупатель = false;
            УказатьПричинуСписания =false;
        }

        public PageShipments(DocShipments doc, Action closePage)
        {
            InitializeComponent();
            _Doc = doc;
            ClosePage = closePage;
            Title = doc.Number + "/" + doc.DataCreate.ToString();
            Clear_UI();
        }
        /// <summary>
        /// Оформляем продажи
        /// </summary>
        public PageShipments(DocSales doc, Action closePage) 
        {
            InitializeComponent();
            Clear_UI();
            ЕстьПокупатель = true;
            Warehouse_From.Content = doc.Warehouse_From.ToString();
            _Doc = doc;
            ClosePage = closePage;
            Title = doc.Number + "/" + doc.DataCreate.ToString();

        }
        /// <summary>
        /// Оформляем перемещение
        /// </summary>
        public PageShipments(DocMoving doc, Action closePage)
        {
            InitializeComponent();
            _Doc = doc;
            ClosePage = closePage;
            Title = doc.Number + "/" + doc.DataCreate.ToString();
            Clear_UI();
            СоСкладаНаСклад = true;
            Warehouse_From.Content = doc.Warehouse_From.NameWarehouse;
            Cbox_warehouses_To.DisplayMemberPath = "NameWarehouse";
            Cbox_warehouses_To.ItemsSource = MDL.MyDataBase.Warehouses;
        }
        /// <summary>
        /// Списание
        /// </summary>
        public PageShipments(DocWriteDowns doc, Action closePage)
        {
            InitializeComponent();
            _Doc = doc;
            ClosePage = closePage;
            Title = doc.Number + "/" + doc.DataCreate.ToString();
            Clear_UI();
            Warehouse.Content = doc.Warehouse.ToString();
            УказатьПричинуСписания = true;
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
                Padding = new(5, 0, 5, 5),
                Content = obj.NamePseudonym
            };
            Label l3 = new()
            {
                Width = 60,
                Height=30,
                Padding = new(5, 1, 5, 5),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Background = MDL.BrushConv("#FFF4F7D0"),
                Content =  Math.Round(obj.CubatureAll, 2)
            };
            if (obj.Product.SyncError) l1.Foreground = Brushes.Red;
            sp.Children.Add(l1);
            Brush brush = obj.Operation switch
            {
                SqlRequest.OperatonEnum.vsPlus => MDL.BrushConv("#FF029E88")!,
                SqlRequest.OperatonEnum.vsMunis => MDL.BrushConv("#FFD39B9B")!,
                SqlRequest.OperatonEnum.vsMutation => Brushes.Blue,
                _ => Brushes.Blue,
            };
            string sts_cont = obj.Operation switch
            {
                SqlRequest.OperatonEnum.vsMunis => "-",
                SqlRequest.OperatonEnum.vsPlus => "+",
                SqlRequest.OperatonEnum.vsMutation => "=",
                _ => ""
            };
            Label l0 = new()
            {
                Width = 43,
                Padding = new(5, 1, 5, 5),
                Foreground = brush,
                Content = sts_cont + Math.Round(obj.SelectedCub, 2)
            };
            sp.Children.Add(l0);
            sp.Children.Add(l3);
            TextBox text1 = new()
            {
                Width = 80,
                Text = obj.PriceCng.ToString(),
                BorderBrush = MDL.BrushConv("#FFD2D9F3"),
                Tag = sp
            };
                       
            sp.Children.Add(text1);            
            Label amount_label = new()
            {
                Padding = new(5, 1, 5, 5),
                Content = obj.Amount
            };
            text1.TextChanged += (object sender, TextChangedEventArgs e) =>
            {      
                    if (double.TryParse(text1.Text, out double di) == false)
                    {
                        di = 0;
                        text1.Text = "";
                    }
                    obj.PriceCng = di;
                    amount_label.Content = obj.Amount;              
                Label_SumDown.Content = _Doc.MainTabel[ListGrup.SelectedIndex].Tabels.Sum(x => x.Amount);
            };
            text1.GotFocus += (object sender, RoutedEventArgs e)=> text1.Text = "";
            text1.LostFocus += (object sender, RoutedEventArgs e) => { if (text1.Text == "") { text1.Text = obj.Product.Price.ToString(); } };

            sp.Tag = obj;
            sp.Children.Add(amount_label);           
            ListBoxEmp.Items.Add(sp);
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
            if (SelectGrup == null)
            {
                MessageBox.Show("Сначало нужно выбрать группу");
                return;
            }

            ButtonAdd.ContextMenu.IsOpen = true;
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

        private bool ПроверитьВходныеДанныеНазванияГруппы() 
        { 
             if (string.IsNullOrEmpty(TBox_GrupName.Text)) return false;
            if (_Doc.MainTabel.Any(x => x.NameGrup == TBox_GrupName.Text))
            {
                MessageBox.Show("Группа с таким названием уже существует ");
                return false;
            }
            // обновляет список MDL если нет значения в списке
            if (!MDL.MyDataBase.NamesGrup.Any(x => x == TBox_GrupName.Text))
            {
                MDL.MyDataBase.NamesGrup.Add(TBox_GrupName.Text);
                TBox_GrupName.Items.Refresh();
            }
            return true;
        }

        private void ДобавитьГруппуСписок()
        {
            if (ПроверитьВходныеДанныеНазванияГруппы() == false) return;
            _Doc.MainTabel.Add(new GrupObj() { NameGrup = TBox_GrupName.Text });
            Refreh_ListGrup();
            ЗакрытьОкноГруппы(null!, null!);
        }

        private void ИзменитьНазаниеГруппы()
        {
            if (ПроверитьВходныеДанныеНазванияГруппы() == false) return;
            if (ListGrup.SelectedIndex != -1) _Doc.MainTabel[ListGrup.SelectedIndex].NameGrup = TBox_GrupName.Text;
            Refreh_ListGrup();
            ЗакрытьОкноГруппы(null!, null!);
        }

        private void КлавишаДобовитьГруппу(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ДобавитьГруппуСписок();
        }

        private void УдалитьОбъект(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ListGrup.SelectedIndex == -1)
            {
                MessageBox.Show("Сначало нужно выбрать группу");
                return;
            }
            if (ListBoxEmp.SelectedIndex != -1)
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
            else if (_Doc is DocWriteDowns) XmlProtocol.SaveDocXml<DocWriteDowns>(_Doc);
            else if (_Doc is DocSales) XmlProtocol.SaveDocXml<DocSales>(_Doc);
            SavedDoc = true;
        }

        private void ДобавитьГруппу(object sender, System.Windows.RoutedEventArgs e)
        {
            RenameFoo = ДобавитьГруппуСписок;
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
            RenameFoo = ИзменитьНазаниеГруппы;
            Grid_NameGrup.Visibility = Visibility.Visible;
            TBox_GrupName.Text = _Doc.MainTabel[ListGrup.SelectedIndex].NameGrup;
            TBox_GrupName.Focus();
            
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
            if (MDL.MainWindow is not null) MDL.MainWindow.FrameWorkspace.Visibility = Visibility.Collapsed;
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
      
        private void ИзменитьНазваниеСтроки() {
            if (_Doc.MainTabel[ListGrup.SelectedIndex].Tabels[ListBoxEmp.SelectedIndex] is Pseudonym pseudonym)
            { 
                pseudonym.NamePseudonym = TBox_GrupName.Text;
                ЗакрытьОкноГруппы(null!, null!);
                Refreh_ListBoxEmp();
            }
         }
        
        private void ПереименоватьСтроку(object sender, RoutedEventArgs e)
        {
            if (_Doc.MainTabel[ListGrup.SelectedIndex].Tabels[ListBoxEmp.SelectedIndex] is Pseudonym pseudonym)
            {
                RenameFoo = ИзменитьНазваниеСтроки;
                Grid_NameGrup.Visibility = Visibility.Visible;
                TBox_GrupName.Text = pseudonym.NamePseudonym;
                TBox_GrupName.Focus();
            }
        }

        private void ДействиГруппой(object sender, RoutedEventArgs e)
        {
            RenameFoo?.Invoke();
        }

        private void РедакторНакладной(object sender, RoutedEventArgs e)
        {

            DocSales doc = ((DocSales)_Doc);

            if (((DocSales)_Doc).Buyer == null)
            {
                MessageBox.Show("Не выбран покупатель");
                return;
            }

            if (MDL.MyDataBase.CompanyOwn == null)
            {
                MessageBox.Show("Для создания документа, требуется указать в настройках программы, вашу компанию и указать реквизиты ");
                return;
            }
            Invoice invoice = new ()
            {
                Payer = doc.Buyer!,
                Shipper = MDL.MyDataBase.CompanyOwn,
                Supplier = doc.Buyer!
            };
            foreach (var item in doc.ListPseudonym)
            {
                invoice.Tabel.Add(new RowTabel()
                {
                    Goods_Name = item.NamePseudonym,                   
                    Price = item.PriceCng,
                    Quantity = item.SelectedCub    
                    // тип ед измер устанавливаеться отдельно из файла
                });
            }

            var wpf = new WpfFrm.WpfView(new InvoiceEditor(invoice), Title, () => { });
            wpf.Show();

        }

        private void ВыборКомпании(object sender, RoutedEventArgs e)
        {
            ToolList<Model.Catalog.Company> toolList = new(MainGrid, (obj) => {
                if (obj is Model.Catalog.Company comp)
                {
                    TBox_Buyer.Text = comp.ToString();
                    if (_Doc is DocSales doc) doc.Buyer = comp;
                }
            });
        }
    }
}
