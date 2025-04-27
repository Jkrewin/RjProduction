using RjProduction.Model;
using RjProduction.Sql;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using RjProduction.Model.DocElement;
using static RjProduction.Sql.SqlRequest;


namespace RjProduction.Pages
{
    [Obsolete]
    public partial class PageRemains : Page
    {
        const string PLUS_COLOR = "#FF3D8F18";
        const string MINUS_COLOR = "#FFAD2F2F";
        const string EQUALLY_COLOR = "#FF2304FF";
        readonly Dictionary<long, WarehouseClass> WarehouseHub = []; // нужен для создания Products
        DataTable? ProdData;
      
        private readonly ObservableCollection<MyCollection> _db = [];
        private ObservableCollection<MyCollection> Db_selected
        {
            get
            {
                var result = (from tv in _db where tv.Selected_Cubature != 0 | (tv.IsOperation ==  OperatonEnum.vsMutation & tv.Selected_Cubature == 0) select tv).ToArray();
                return [.. result];
            }
        }

        public PageRemains() => InitializeComponent();

        private class MyCollection(DataRow row, Dictionary<long, WarehouseClass> warehouseHub) :  INotifyPropertyChanged
        {
            private int _Piece = 0;
            private double _Selected_Cubature = 0;
            private OperatonEnum _IsOperation =  OperatonEnum.vsPlus;
            private double _Result = 0;

            public Model.Products Products = new (row, warehouseHub);
            public event PropertyChangedEventHandler? PropertyChanged;


            public string NameItem { get => Products.NameItem; }
            public string TypeWood { get => Products.TypeWood.ToString(); }
            public string Cubature { get => Math.Round( Products.Cubature,3).ToString(); }

            public double Selected_Cubature
            {
                get => _Selected_Cubature; set
                {
                    _Selected_Cubature = value;
                    RefrehResult();
                    NotifyPropertyChanged();
                }
            }

            public double Result
            {
                get => Math.Round( _Result,2);
                set
                {
                    _Result = value;
                    NotifyPropertyChanged();
                }
            }

            public OperatonEnum IsOperation
            {
                get => _IsOperation;
                set
                {
                    _IsOperation = value;
                    NotifyPropertyChanged();
                }
            }

            public Model.DocElement.Pseudonym ToPseudonym()
            {
                return new Model.DocElement.Pseudonym() { SelectedCub = Selected_Cubature,
                    Product = Products,
                    Operation = IsOperation ,                     
                    PriceCng = Products.Price
                };
            }

            public int Piece
            {
                get => _Piece;
                set
                {
                    _Piece = value;
                    _Selected_Cubature = Products.OnePice * _Piece;
                    RefrehResult();
                    NotifyPropertyChanged();
                }
            }

            public void RefrehResult()
            {
                if (IsOperation == OperatonEnum.vsMunis)
                {
                    Result = Products.Cubature - Selected_Cubature;
                    if (Result < 0) Result = 0;
                }
                else if (IsOperation == OperatonEnum.vsMutation) {
                    Result = Selected_Cubature;
                    if (Result < 0) Result = 0;
                }
                else
                {
                    Result = Products.Cubature + Selected_Cubature;
                }

            }

            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        /// <summary>
        /// Выполнить после закрытия формы к примеру действия обновить таблицу
        /// </summary>
        private void Close_action() {
            DockPanel_РамкаДокумента.Visibility = Visibility.Collapsed;
            ProdData = SqlRequest.GetDataTable(nameof(Products)); // обновить данные
            ВыбраннаСтрока(null!, null!);
        }
        /// <summary>
        /// Начальная сборка документа
        /// </summary>
        /// <param name="grups">List<GrupObj></param>
        /// <returns>true - без ошибок</returns>
        private bool First_Const(out List<GrupObj> grups ) {
            grups = [];
            // проверка на ноль элементов
            foreach (var item in Db_selected)
            {
                if (item.IsOperation == OperatonEnum.vsMunis & (item.Products.Cubature - item.Selected_Cubature) < 0)
                {
                    MessageBox.Show($"{item.Products.NameItem} не может быть меньше нуля. Отрицательный остаток не допускаеться");
                    return false;
                }
            }
            // Сборка превдонимов в таблицу
            foreach (var item in Db_selected)
            {
                if (!grups.Any(x => x.NameGrup == item.Products.Warehouse.NameWarehouse))
                {
                    grups.Add(new GrupObj() { NameGrup = item.Products.Warehouse.NameWarehouse });
                    grups.Last().Tabels.Add(item.ToPseudonym());
                }
                else
                {
                    var f = grups.Find(x => x.NameGrup == item.Products.Warehouse.NameWarehouse);
                    f!.Tabels.Add(item.ToPseudonym());
                }
            }
            return true;
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {           
            if (MDL.SqlProfile is null)
            {
                Label_NotDB.Visibility = Visibility.Visible;
                return;
            }

            DataTable? wh;
            wh = SqlRequest.GetDataTable(nameof(WarehouseClass));
            if (wh != null)
            {
                foreach (DataRow row in wh.Rows)
                {
                    WarehouseClass warehouse = new(row);
                    WarehouseHub.Add(warehouse.ID, warehouse);
                }    

                MainComboBox.ItemsSource = WarehouseHub.Values.ToList();
                MainComboBox.DisplayMemberPath = "NameWarehouse";

                DG_Main.ItemsSource = _db;
            }

            //for (int i = 7; i < DG_Main.Columns.Count; i++) { DG_Main.Columns[i].Visibility = Visibility.Collapsed; }
            Скрыть_Меню(null!, null!);
        }
              
        private void ВыбратьЗнак(object sender, RoutedEventArgs e)
        {
            var b = ((Button)sender);
            if (DG_Main.SelectedItem is MyCollection my)
            {
                if (b.Content.Equals(""))
                {
                    b.Content = "";
                    b.Background = MDL.BrushConv(MINUS_COLOR);
                    my.IsOperation = OperatonEnum.vsMunis;
                    my.RefrehResult();
                }
                else if (b.Content.Equals(""))
                {
                    b.Content = "";
                    b.Background = MDL.BrushConv(EQUALLY_COLOR);
                    my.IsOperation = OperatonEnum.vsMutation;
                    my.RefrehResult();
                }
                else if (b.Content.Equals(""))
                {
                    b.Content = "";
                    b.Background = MDL.BrushConv(PLUS_COLOR);
                    my.IsOperation = OperatonEnum.vsPlus;
                    my.RefrehResult();
                }
            }
        }

        private void ВыбраннаСтрока(object sender, SelectionChangedEventArgs e)
        {
            if (MainComboBox.SelectedItem is not WarehouseClass warehouse) return;
            
            string id = warehouse.ID.ToString();
            _db.Clear();
            ProdData = SqlRequest.GetDataTable(nameof(Products),"*", "Warehouse='"+ id+"'"); // загрузить сюда весь список Products
            if (ProdData is null) return;

            foreach (DataRow tv in ProdData.Rows) _db.Add(new MyCollection(tv, WarehouseHub));
        }

        private void СоздатьДокумент(object sender, RoutedEventArgs e)
        {
            SelectWarehouse.Visibility = Visibility.Hidden;
            DG_Main.Visibility = Visibility.Hidden;
            SP_Select_Type_Doc.Visibility = Visibility.Visible;
            StartGrid.RowDefinitions[1].Height = new GridLength(1);
        }
                
        private void Скрыть_Меню(object sender, RoutedEventArgs e)
        {
            SelectWarehouse.Visibility = Visibility.Visible;
            DG_Main.Visibility = Visibility.Visible;
            SP_Select_Type_Doc.Visibility = Visibility.Hidden;
            StartGrid.RowDefinitions[1].Height = new GridLength(100);
        }

        private void ДокументВыравнитьОстаток(object sender, RoutedEventArgs e)
        {
            if (First_Const(out List<GrupObj> tabel))
            {
                var page = new Pages.Doc.PageShipments(new XML.DocShipments() { DocTitle = "Выравнивание Остатков", Number = MDL.MyDataBase.NumberDef + 1,  MainTabel = tabel }, DockPanel_РамкаДокумента, Close_action);
                DockPanel_РамкаДокумента.Visibility = Visibility.Visible;
                FrameDisplay.Navigate(page);

                Скрыть_Меню(null!, null!);
            }
        }

        private void Поиск(object sender, TextChangedEventArgs e)
        {
            string str = ((TextBox)sender).Text.ToLower();
            if (str.Length > 1)
            {
                if (MainComboBox.SelectedItem is not WarehouseClass warehouse) return;
                if (ProdData == null) return;
                string id = warehouse.ID.ToString();

                _db.Clear();
                foreach (DataRow tv in ProdData.Rows)
                {
                    if (tv["Warehouse"].ToString() == id & tv["NameItem"].ToString()!.Length != tv["NameItem"].ToString()!.ToLower().Replace(str, "").Length)
                    {
                        _db.Add(new MyCollection(tv, WarehouseHub));
                    }
                }
            }
            else ВыбраннаСтрока(null!, null!);
        }
        /// <summary>
        /// Проверка на наличие только минуса 
        /// </summary>
        /// <returns></returns>
        private bool Only_Munis() {
            // проверка чтобы были только -
            foreach (var item in Db_selected)
            {
                if ((item.IsOperation == OperatonEnum.vsPlus | item.IsOperation == OperatonEnum.vsMutation) & item.Selected_Cubature != 0)
                {
                    MessageBox.Show($"{item.Products.NameItem} не может быть + или =. Для категории [со склада на склад] все выбранные элементы могут быть только -");
                    return false;
                }
            }
            return true;
        }

        private void ДокументПеремещение(object sender, RoutedEventArgs e)
        {
            if (!Only_Munis()) return;
            // проверка на отрицательные числа выравнивает их до 0
            foreach (var item in Db_selected)
            {
                if ((item.Products.Cubature - item.Selected_Cubature) < 0) item.Selected_Cubature = item.Products.Cubature;
            }
            // проверка на ругие склады тоько один может быть 
            WarehouseClass? w = null;
            foreach (var item in Db_selected)
            {
                if (w == null) w = item.Products.Warehouse;
                else if (w.Equals(item.Products.Warehouse) == false)
                {
                    MessageBox.Show($"У васм выбрана продукция из разных складов. Только один склад может быть выбран для данной категории документа. ");
                    return;
                }
            }

            if (MainComboBox.SelectedItem is WarehouseClass warehouse) {
                if (First_Const(out List<GrupObj> tabel))
                {
                    var page = new Pages.Doc.PageShipments(new XML.DocMoving() { DocTitle = "Со склада на склад", Number = MDL.MyDataBase.NumberDef + 1, MainTabel = tabel, Warehouse_From = warehouse }, DockPanel_РамкаДокумента, Close_action);
                    DockPanel_РамкаДокумента.Visibility = Visibility.Visible;
                    FrameDisplay.Navigate(page);

                    Скрыть_Меню(null!, null!);
                } }
        }

        private void Удалить_значение(object sender, RoutedEventArgs e)
        {
            if (DG_Main.SelectedItem is MyCollection my)
            {
                if (MessageBox.Show("Удалить строку " + my.Products.NameItem + "? Будет удалена в случае если остатки равны нулю.", "", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
                if (MDL.SqlProfile is not null)
                {
                    try
                    {
                        MDL.SqlProfile.Conection();
                        MDL.SqlProfile.Delete(nameof(Model.Products), $"ID ={my.Products.ID} AND {nameof(Products.Cubature)} =0");
                    }
                    catch (Exception ex)
                    {
                        MDL.LogError("Ошибка при удаление строки", ex.Message + ex.Source);
                    }
                    finally
                    {
                        MDL.SqlProfile.Disconnect();
                    }
                }
                ВыбраннаСтрока(null!, null!);
            }
        }

        private void ТекстОбновленКубы(object sender, TextChangedEventArgs e)
        {
           
        }

        private void ТекстОбновленКоличест(object sender, TextChangedEventArgs e)
        {
            if (DG_Main.SelectedItem is MyCollection my)
            {
                if (int.TryParse(((TextBox)sender).Text, out int i))
                    my.Piece = i;
            }
        }

        private void Списание(object sender, RoutedEventArgs e)
        {
            if (First_Const(out List<GrupObj> tabel))
            {
                var page = new Pages.Doc.PageShipments(new XML.DocWriteDowns() { DocTitle = "Списание товара", Number = MDL.MyDataBase.NumberDef + 1, MainTabel = tabel }, DockPanel_РамкаДокумента, Close_action);
                DockPanel_РамкаДокумента.Visibility = Visibility.Visible;
                FrameDisplay.Navigate(page);

                Скрыть_Меню(null!, null!);
            }
        }

        private void Продажи(object sender, RoutedEventArgs e)
        {
            if (!Only_Munis()) return;

            if (First_Const(out List<GrupObj> tabel))
            {
                var page = new Pages.Doc.PageShipments(new XML.DocSales() { 
                    Warehouse_From = (WarehouseClass)MainComboBox.SelectedItem,
                    DocTitle = "Продажа продукции", 
                    Number = MDL.MyDataBase.NumberDef + 1, 
                    MainTabel = tabel 
                }, DockPanel_РамкаДокумента, Close_action);
                DockPanel_РамкаДокумента.Visibility = Visibility.Visible;
                FrameDisplay.Navigate(page);

                Скрыть_Меню(null!, null!);
            }
        }

        private void ОбновитьТекстКубах(object sender, RoutedEventArgs e)
        {
            string str = ((TextBox)sender).Text.Replace('.', ',');
            if (double.TryParse(str, out double d))
            {
                ((MyCollection)DG_Main.SelectedItem).Selected_Cubature = d;
            }
        }
    }
}
