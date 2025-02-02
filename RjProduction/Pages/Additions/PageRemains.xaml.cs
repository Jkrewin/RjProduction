using RjProduction.Model;
using RjProduction.Sql;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace RjProduction.Pages
{
    public partial class PageRemains : Page
    {
        const string PLUS_COLOR = "#FF3D8F18";
        const string MINUS_COLOR = "#FFAD2F2F";
        readonly Dictionary<long, WarehouseClass> WarehouseHub = []; // нужен для создания Products
        DataTable? ProdData;
        bool _swither = true;

        private readonly ObservableCollection<MyCollection> _db = [];
        private ObservableCollection<MyCollection> Db_selected
        {
            get
            {
                var result = (from tv in _db where tv.Selected_Cubature != 0  select tv).ToArray();
                return new ObservableCollection<MyCollection> (result);
            }
        }

        public PageRemains() => InitializeComponent();

        private class MyCollection(DataRow row, Dictionary<long, WarehouseClass> warehouseHub) :  INotifyPropertyChanged
        {
            private double _Selected_Cubature = 0;
            private bool _IsMunus = false;
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

            public bool IsMunus
            {
                get => _IsMunus;
                set
                {
                    _IsMunus = value;
                    NotifyPropertyChanged();
                }
            }

            public Model.Pseudonym ToPseudonym()
            {
                return new Pseudonym() { SelectedCub = Selected_Cubature,
                    Product = Products,
                    Operation = IsMunus ? SqlRequest.OperatonEnum.vsMunis : SqlRequest.OperatonEnum.vsPlus ,                     
                    PriceCng = Products.Price
                };
            }

            public void RefrehResult()
            {
                if (IsMunus == true)
                {
                    Result = Products. Cubature - Selected_Cubature;
                    if (Result < 0) Result = 0;
                }
                else
                {
                    Result = Products.Cubature + Selected_Cubature;
                }

            }

            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        #region "Ридер строк таблици по UI"
        private DataGridCell? GetCell(int row, int column)
        {
            DataGridRow? rowContainer = GetRow(row);
            if (rowContainer != null)
            {
                DataGridCellsPresenter? presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter == null)
                {
                    DG_Main.ScrollIntoView(rowContainer, DG_Main.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                }
                if (presenter is not null)
                {
                    DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                    return cell;
                }
            }
            return null;
        }
        public static T? GetVisualChild<T>(Visual parent) where T : Visual
        {
            T? child = default;
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                child ??= GetVisualChild<T>(v);
                if (child != null) break;
            }
            return child;
        }
        private DataGridRow? GetRow(int index)
        {
            DataGridRow row = (DataGridRow)DG_Main.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                DG_Main.UpdateLayout();
                DG_Main.ScrollIntoView(DG_Main.Items[index]);
                row = (DataGridRow)DG_Main.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }
        #endregion

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
                if (item.IsMunus & (item.Products.Cubature - item.Selected_Cubature) < 0)
                {
                    MessageBox.Show($"{item.Products.NameItem} не может быть меньше нуля. Отрицательный остаток не допускаеться");
                    return false;
                }
            }
          
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

        private void ВыборОбъекта(object sender, SelectedCellsChangedEventArgs e)
        {

        }

        private void ВыбратьЗнак(object sender, RoutedEventArgs e)
        {
            var b = ((Button)sender);
            if (b.Content.Equals(""))
            {
                b.Content = "";
                b.Background = MDL.BrushConv(MINUS_COLOR);
                if (DG_Main.SelectedItem is MyCollection my)
                {
                    my.IsMunus = true;
                    my.RefrehResult();
                }
            }
            else
            {
                b.Content = "";
                b.Background = MDL.BrushConv(PLUS_COLOR);
                if (DG_Main.SelectedItem is MyCollection my)
                {
                    my.IsMunus = false;
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

            foreach (DataRow tv in ProdData.Rows)
            {               
                    _db.Add(new MyCollection(tv, WarehouseHub));                
            }

        }

        private void СоздатьДокумент(object sender, RoutedEventArgs e)
        {
            SelectWarehouse.Visibility = Visibility.Hidden;
            DG_Main.Visibility = Visibility.Hidden;
            SP_Select_Type_Doc.Visibility = Visibility.Visible;


        }

        private void ТекстОбновлен(object sender, TextChangedEventArgs e)
        {
            const int TEXTCUBS = 4; // индекс контрола

            if (!_swither) return;
            if (DG_Main.SelectedItem is MyCollection my)
            {
                var text = (TextBox)sender;

                if (text.Name == "TextCubs")
                {
                    if (text.Text.Length != 0)
                    {
                        if (double.TryParse(text.Text, out double d)) my.Selected_Cubature = d;
                        else text.Text = my.Selected_Cubature.ToString();
                    }
                }
                else if (text.Name == "TextQ")
                {
                    if (uint.TryParse(text.Text, out uint q))
                    {
                        var cell = GetCell(DG_Main.SelectedIndex, TEXTCUBS);
                        if (GetVisualChild<TextBox>(cell!) is TextBox b)
                        {
                            my.Selected_Cubature = my.Products.OnePice * q;
                            _swither = false;
                            b.Text = my.Selected_Cubature.ToString();
                            _swither = true;
                        }
                    }
                    else text.Text = "0";
                }
                my.RefrehResult();
            }
        }

        private void Скрыть_Меню(object sender, RoutedEventArgs e)
        {
            SelectWarehouse.Visibility = Visibility.Visible;
            DG_Main.Visibility = Visibility.Visible;
            SP_Select_Type_Doc.Visibility = Visibility.Hidden;
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

        private void ДокументПеремещение(object sender, RoutedEventArgs e)
        {
            // проверка чтобы были только -
            foreach (var item in Db_selected)
            {
                if (item.IsMunus == false & item.Selected_Cubature != 0)
                {
                    MessageBox.Show($"{item.Products.NameItem} не может быть +. Для категории [со склада на склад] все выбранные элементы могут быть только -");
                    return;
                }
            }
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
    }
}
