using RjProduction.Model;
using RjProduction.XML;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace RjProduction.Pages
{
    public partial class PageDocEditor : Page
    {
        private GrupObj? SelectGrup;        //Выбранная группа
        private readonly DocArrival MyDoc;    //Редактируемый документ
        private bool RenameGrup = false;    //Режим переименование группы
        private readonly Action ClosePage;  //Ссылка на метод закрытия это формы
        private bool SavedDoc = false;      //Документ пока не сохранялся
        private object? _obj;               //Page с доб и изм строк
        private readonly UIElement? MainFramePanel;

        public PageDocEditor(DocArrival doc, Action closePage)
        {
            InitializeComponent();
            MyDoc = doc;
            ClosePage = closePage;
        }
        public PageDocEditor(DocArrival doc, UIElement framePanel, Action closePage)
        {
            InitializeComponent();
            MyDoc = doc;
            ClosePage = closePage;
            MainFramePanel = framePanel;
        }

        /// <summary>
        /// Обновить список в основной части MainTabel
        /// </summary>
        private void Refreh_Tabels()
        {
            // формирование списка групп
            ListGrup.Items.Clear();
            foreach (var item in MyDoc.MainTabel)
            {
                if (SelectGrup != null && SelectGrup.NameGrup == item.NameGrup) ListGrup.Items.Add("> " + item.NameGrup + " <");
                else ListGrup.Items.Add(item.NameGrup);
            }

            // формирования списка что входит в группу
            ListBoxEmp.Items.Clear();
            if (SelectGrup == null) return;

            // Подготовка к анализу
            int salaries = 0;
            double cuba = 0;
            decimal allAmount = 0;
            double surch_price = 0;

            // Тут доплаты
            List<Surcharges> surcharges = [];
            foreach (var item in SelectGrup!.Tabels) {
                if (item is Surcharges s) {
                    if (s.TypeSurcharges == Surcharges.TypeSurchargesEnum.ДоплатаЦене) surch_price += s.UpRaise;
                    surcharges.Add(s); 
                }                
            }

            // Увеличить цена доплатами
            for (int i = 0; i < SelectGrup!.Tabels.Count; i++)
            {
                if (SelectGrup.Tabels[i] is MaterialObj m )
                {
                    SelectGrup.Tabels[i] = m with { UpRaise = surch_price };
                }
                else if (SelectGrup.Tabels[i] is Tabel_Timbers h)
                {
                    for (int t = 0; t < h.Timbers.Count; t++)
                    {
                        var tmp = h.Timbers[t];
                        tmp. UpRaise = surch_price ;
                    }
                }
            }

            double surch_sum(string name)
            {
                double surchargesPrice = 0;

                foreach (var s in surcharges)
                {
                    if (s.TypeSurcharges == Surcharges.TypeSurchargesEnum.ДоплатаСумме)
                    {
                        if (string.IsNullOrEmpty(s.EmployeeName))
                        {
                            surchargesPrice += s.UpRaise;
                        }
                        else if (s.EmployeeName.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                        {
                            surchargesPrice += s.UpRaise;
                        }
                    }
                }
                return surchargesPrice;
            }

            // Общая сумма и кубы
            foreach (var item in SelectGrup!.Tabels)
            {
                if (item is Employee e) 
                { 
                    if (e.Worker) salaries++; 
                }
                else if (item is MaterialObj m)
                {                   
                    allAmount += m.Amount;
                    cuba += m.CubatureAll;
                }
                else if (item is Tabel_Timbers h) {                   
                    allAmount += h.Amount; 
                    cuba += h.CubatureAll; 
                }
            }
                     
            // Задействоан в производстве делить прибыль 50/50 между рабочими  
            Label_SumUP.Content = allAmount.ToString(); // Сумма дохода
            if (salaries > 0)
            {
                allAmount /= salaries;
                if (MDL.SetApp.RoundingAmountsEmpl)
                {
                    int am = (int)allAmount;
                    double y = (double)(allAmount - am);
                    y = y > 0.51 ? 1 : 0;
                    allAmount = (decimal)(am + y);
                }
                for (int i = 0; i < SelectGrup.Tabels.Count; i++)
                {
                    if (SelectGrup.Tabels[i] is Employee e)
                    {
                        e.UpRaise  = (double)surch_sum(e.NameEmployee); // доб доплаты по сумме
                        if (e.Worker)
                        {                           
                            SelectGrup!.Tabels[i] = e with { Payment = (double)allAmount };
                        }
                    }
                }
            }

            Label_CUBA.Content = cuba == 0 ? "--" : Math.Round(cuba, 3);    // Сколько всего кубов
            Label_SumDown.Content = (from tv in SelectGrup.Tabels where tv is Employee select tv).Sum(x => x.Amount); // Сумма зарплат
            foreach (var item in SelectGrup!.Tabels) ListBoxEmp.Items.Add(item.ToString());
        }

        private void OpenWpfItem(IDoc doc, int edit = -1)
        {

            void actor (IDoc iDoc) {
                if (edit == -1) SelectGrup!.Tabels.Add(iDoc);
                else SelectGrup!.Tabels[edit] = iDoc;
                Refreh_Tabels();
            }

            Grid_Curtain.Visibility = Visibility.Visible;
            FrameDisplay.NavigationService.RemoveBackEntry();

            if (doc is MaterialObj mm) 
            {
                _obj = new PageLumber(mm, actor,  
                    () => Grid_Curtain.Visibility = Visibility.Collapsed);

                FrameDisplay.Height= ((Page)_obj).Height ;
                FrameDisplay.Navigate(_obj);               
            }
            else if (doc is Employee empl) {
                _obj = new PageStaff(empl, actor,
                      () => Grid_Curtain.Visibility = Visibility.Collapsed);

                FrameDisplay.Height = 229;
                FrameDisplay.Navigate(_obj);
            }
            else if (doc is Tabel_Timbers tabel)
            {
                _obj = new PageTimbers(tabel, actor,
                   () => Grid_Curtain.Visibility = Visibility.Collapsed);

                FrameDisplay.Height = 450 ;
                FrameDisplay.Navigate(_obj);
            }
            else if (doc is Surcharges sur)
            {
                _obj = new PageSurcharges(sur, actor, () => Grid_Curtain.Visibility = Visibility.Collapsed)
                {
                    Height = 177
                };
                FrameDisplay.Height = 180;
                FrameDisplay.Navigate(_obj);

            }
        }

        private void ДобавитьОбъект(object sender, RoutedEventArgs e)
        {
            if (SelectGrup == null)
            {
                MessageBox.Show("Сначало нужно выбрать группу");
                return;
            }

            ButtonAdd.ContextMenu.IsOpen = true;
        }

        private void УдалитьОбъект(object sender, RoutedEventArgs e)
        {
            if (SelectGrup == null)
            {
                MessageBox.Show("Сначало нужно выбрать группу");
                return;
            }
            if (ListBoxEmp.SelectedItem != null)
            {
                SelectGrup.Tabels.RemoveAt(ListBoxEmp.SelectedIndex);
                Refreh_Tabels();
            }
            else
            {
                MessageBox.Show("Сначало нужно выбрать объект из списка ниже");
            }
        }

        private void ДобавитьГруппу(object sender, RoutedEventArgs e)
        {
            Grid_NameGrup.Visibility = Visibility.Visible;
            TBox_GrupName.Focus();
        }

        private void ДобавитьГруппуСписок(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TBox_GrupName.Text)) return;
            if (MyDoc.MainTabel.Any(x => x.NameGrup == TBox_GrupName.Text))
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

            if (RenameGrup == true & SelectGrup != null)
            {
                SelectGrup!.NameGrup = TBox_GrupName.Text;
                RenameGrup = false;
            }
            else MyDoc.MainTabel.Add(new GrupObj() { NameGrup = TBox_GrupName.Text });
            SelectGrup = MyDoc.MainTabel[^1];
            Refreh_Tabels();
            ЗакрытьОкноГруппы(null!, null!);
        }       

        private void ВыбраннаГруппа(object sender, SelectionChangedEventArgs e)
        {
            if (ListGrup.SelectedItem == null) return;
            var s = MyDoc.MainTabel.Find(x => x.NameGrup == ListGrup.SelectedItem.ToString());
            if (s != null)
            {
                SelectGrup = MyDoc.MainTabel.Find(x => x.NameGrup == ListGrup.SelectedItem.ToString());
                Refreh_Tabels();
            }
        }

        private void КлавишаДобовитьГруппу(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ДобавитьГруппуСписок(null!, null!);
        }

        private void УдалитьГруппу(object sender, RoutedEventArgs e)
        {
            if (SelectGrup == null) return;
            MyDoc.MainTabel.Remove(SelectGrup);
            Refreh_Tabels();
        }

        private void СохранитьXML(object sender, RoutedEventArgs e)
        {            
            if (DataCreate.SelectedDate.HasValue == false)
            {
                MessageBox.Show("Укажите дату документа.");
                return;
            }
            if (Cbox_warehouses.SelectedItem is null)
            {
                MessageBox.Show("Укажите склад.");
                return;
            }

            MyDoc.Warehouse = (WarehouseClass)Cbox_warehouses.SelectedItem;
            MDL.MyDataBase.WarehouseDef = (Model.WarehouseClass?)Cbox_warehouses.SelectedItem;

            XmlProtocol.SaveDocXml<DocArrival>(MyDoc);
            SavedDoc = true;
            ((Button)sender).Foreground = MDL.BrushConv("#FFFFFFFF");
        }

        private void ПереименоватьГруппу(object sender, RoutedEventArgs e)
        {
            if (SelectGrup == null) return;
            Grid_NameGrup.Visibility = Visibility.Visible;
            TBox_GrupName.Text = SelectGrup.NameGrup;
            TBox_GrupName.Focus();
            RenameGrup = true;
            Refreh_Tabels();
        }
               
        private void ВодНомера(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(((TextBox)sender).Text, out uint u)) MyDoc.Number = u;
            else ((TextBox)sender).Text = MyDoc.Number.ToString();
        }
               
        private void КопироватьГруппу(object sender, RoutedEventArgs e)
        {
            if (SelectGrup == null) return;
            GrupObj grup = new() { NameGrup = SelectGrup.NameGrup + " копия" };

            foreach (var item in SelectGrup.Tabels)
            {
                if (item is Tabel_Timbers timbers) {
                   Tabel_Timbers tabel_ = new();
                    foreach (var item2 in timbers.Timbers) tabel_.Timbers.Add((Timber)item2.Clone());
                }
                else grup.Tabels.Add(item);
            }
            MyDoc.MainTabel.Add(grup);
            Refreh_Tabels();
        }       

        private void БыстриыеКлвиши(object sender, KeyEventArgs e)
        {
            if (SelectGrup == null) return;
            if (e.Key == Key.F5) OpenWpfItem(new MaterialObj());
            else if (e.Key == Key.F6) OpenWpfItem(new Employee());
            else if (e.Key == Key.F7) OpenWpfItem(new Tabel_Timbers());
        }

        private void РедактированиеОбъекта(object sender, MouseButtonEventArgs e)
        {
            int idex = ListBoxEmp.SelectedIndex;
            if (idex == -1) return;
            if (SelectGrup == null) return;
            OpenWpfItem(SelectGrup.Tabels[idex], idex);
        }

        private void ДобавитьПМат(object sender, RoutedEventArgs e) => OpenWpfItem(new MaterialObj());
        private void ДобавитьСотрудника(object sender, RoutedEventArgs e) => OpenWpfItem(new Employee());
        private void ДобавитьКруглыйЛес(object sender, RoutedEventArgs e) => OpenWpfItem(new Tabel_Timbers());
        private void ЗакрытьОкноГруппы(object sender, RoutedEventArgs e) => Grid_NameGrup.Visibility = Visibility.Collapsed;
        private void ВыходИзДаты_(object sender, RoutedEventArgs e) => MyDoc.DataCreate = DateOnly.FromDateTime(((DatePicker)sender).SelectedDate!.Value);
        private void ВходВПоле_(object sender, RoutedEventArgs e) => ((TextBox)sender).Text = "";        
        private void ВыбранаГалочка(object sender, RoutedEventArgs e) => Refreh_Tabels();

        private void НажитиеЗакрыть(object sender, RoutedEventArgs e)
        {
            if (SavedDoc == false) {
                var b=  MessageBox.Show("Вы не сохранили документ. Закрыть этот документ без сохранения ? ", "Изменения в документ", MessageBoxButton.YesNo);
                if (b == MessageBoxResult.No) return;
            }
            ClosePage?.Invoke();
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            string[] arr = ["Res/images/masonry_view.png", "Res/images/worker.png", "Res/images/wood_icon.png", "Res/images/coin_icon.png"];
            int i = 0;
            foreach (MenuItem item in ButtonAdd.ContextMenu.Items)
            {
                item.Icon = new Image { Source = new BitmapImage(new Uri(arr[i], UriKind.Relative)) };
                i++;
            }
            TBox_GrupName.ItemsSource = MDL.MyDataBase.NamesGrup;
            var window = Window.GetWindow(this);
            window.KeyDown += БыстриыеКлвиши;

            MDL.ImportToWpf(this, MyDoc);
            Refreh_Tabels();

            Cbox_warehouses.ItemsSource = MDL.MyDataBase.Warehouses;
            Cbox_warehouses.DisplayMemberPath = "NameWarehouse";

            //  Выбрать склад по умолчанию
            if (MDL.MyDataBase.WarehouseDef is not null)
            {
                Cbox_warehouses.SelectedIndex = MDL.MyDataBase.Warehouses.FindIndex(x => x.NameWarehouse == MDL.MyDataBase.WarehouseDef.NameWarehouse);
            }


        }

        private void ДобавитьДоплата(object sender, RoutedEventArgs e)
        {
            OpenWpfItem(new Surcharges());
        }

        private void СоздатьСклад(object sender, RoutedEventArgs e)
        {
            WpfFrm.WpfWarehouse wpf = new(
                () =>
                {
                    Cbox_warehouses.Items.Refresh();
                    if (Cbox_warehouses.Items.Count >= 1) Cbox_warehouses.SelectedIndex = Cbox_warehouses.Items.Count - 1;
                });
            wpf.ShowDialog();

        }

        private void ВыбранСклад(object sender, SelectionChangedEventArgs e) => MyDoc.Warehouse = MDL.MyDataBase.Warehouses.Find(x => x.Equals(Cbox_warehouses.Text)) ?? new WarehouseClass() { NameWarehouse = "NaN" };

        private void ВозвратЦвета(object sender, MouseEventArgs e)
        {
            ((Button)sender).Foreground = MDL.BrushConv("#FF000000");
        }

        private void РазвернутьЭкран(object sender, RoutedEventArgs e)
        {
            CloseWpf.Visibility = Visibility.Collapsed;
            ((Button)sender).Visibility = Visibility.Collapsed;
            if (MainFramePanel is not null) MainFramePanel.Visibility = Visibility.Collapsed;
            var wpf =new WpfFrm.WpfView(this,ClosePage);
            wpf.Show();
        }
    }
}
