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

        private class MyCollection : Model.Products, INotifyPropertyChanged
        {
            private double _Selected_Cubature = 0;
            private bool _IsMunus = false;
            private double _Result = 0;

            public event PropertyChangedEventHandler? PropertyChanged;


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
                get => _Result;
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


            public MyCollection() { }

            public MyCollection(DataRow row, Dictionary<long, WarehouseClass> warehouseHub) : base(row, warehouseHub)
            {

            }

            public Model.Pseudonym ToPseudonym()
            {
                return new Pseudonym() { CubAll = Selected_Cubature,
                    Name = NameItem,
                    OnePice = OnePice, 
                    Operation = IsMunus ? SqlRequest.OperatonEnum.vsMunis : SqlRequest.OperatonEnum.vsPlus , 
                    ID_Prod = this.ID,
                    Price =Price
                };
            }

            public void RefrehResult()
            {
                if (IsMunus == true)
                {
                    Result = Cubature - Selected_Cubature;
                }
                else
                {
                    Result = Cubature + Selected_Cubature;
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
                if (child == null) child = GetVisualChild<T>(v);
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

                ProdData = SqlRequest.GetDataTable(nameof(Products)); // загрузить сюда весь список Products

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
            if (ProdData == null) return;

            string id = warehouse.ID.ToString();
            _db.Clear();

            foreach (DataRow tv in ProdData.Rows)
            {
                if (tv["Warehouse"].ToString() == id)
                {
                    _db.Add(new MyCollection(tv, WarehouseHub));
                }
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
                    if (double.TryParse(text.Text, out double d)) my.Selected_Cubature = d;
                    else text.Text = my.Selected_Cubature.ToString();
                }
                else if (text.Name == "TextQ")
                {
                    if (uint.TryParse(text.Text, out uint q))
                    {
                        var cell = GetCell(DG_Main.SelectedIndex, TEXTCUBS);
                        if (GetVisualChild<TextBox>(cell!) is TextBox b)
                        {
                            my.Selected_Cubature = my.OnePice * q;
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
            // проверка на -число
            foreach (var item in Db_selected)
            {
                if (item.IsMunus & (item.Cubature - item.Selected_Cubature) < 0)
                {
                    MessageBox.Show($"{item.NameItem} не может быть меньше нуля. Отрицательный остаток не допускаеться");
                    return;
                }
            }

            List<GrupObj> tabel = [];
            foreach (var item in Db_selected)
            {
                if (!tabel.Any(x => x.NameGrup == item.Warehouse.NameWarehouse))
                {
                    tabel.Add(new GrupObj() { NameGrup = item.Warehouse.NameWarehouse });
                    tabel.Last().Tabels.Add(item.ToPseudonym());
                }
                else
                {
                    var f = tabel.Find(x => x.NameGrup == item.Warehouse.NameWarehouse);
                    f!.Tabels.Add(item.ToPseudonym());
                }
            }
            //tabel.Add(new GrupObj() { NameGrup = "Автотранспорт" });
            void sysEnd()
            {
                // выполнить после закрытия действия обновить таблицу
                DockPanel_РамкаДокумента.Visibility = Visibility.Collapsed;
                ProdData = SqlRequest.GetDataTable(nameof(Products)); // обновить данные
                ВыбраннаСтрока(null!, null!);
            }
          
            var page = new Pages.Doc.PageShipments(new XML.DocShipments() { DocTitle = "Отгрузка материалов", MainTabel = tabel }, DockPanel_РамкаДокумента, sysEnd);
            DockPanel_РамкаДокумента.Visibility = Visibility.Visible;
            FrameDisplay.Navigate(page);           
           
            Скрыть_Меню(null!, null!);
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
    }
}
