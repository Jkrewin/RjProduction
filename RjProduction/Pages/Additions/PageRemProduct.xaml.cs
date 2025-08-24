using RjProduction.Model;
using RjProduction.Model.DocElement;
using RjProduction.Sql;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using static RjProduction.Sql.SqlRequest;

namespace RjProduction.Pages.Additions
{
    public partial class PageRemProduct : Page
    {
        const string PLUS_COLOR = "#FF3D8F18";
        const string MINUS_COLOR = "#FFAD2F2F";
        const string EQUALLY_COLOR = "#FF2304FF";
        private readonly Dictionary<long, WarehouseClass> WarehouseHub = []; // нужен для создания Products
        private DataTable? ProdData;
        private readonly ObservableCollection<MyCollection> _db = [];
        private ObservableCollection<MyCollection> Db_selected
        {
            get
            {
                var result = (from tv in _db where tv.Selected_Cubature != 0 | (tv.IsOperation == OperatonEnum.vsMutation & tv.Selected_Cubature == 0) select tv).ToArray();
                return [.. result];
            }
        }
        /// <summary>
        /// Дейстие создание по выбранному документу
        /// </summary>
        private Action? SelectedDoc;

        private bool PlusIcon
        {
            get
            {
                return RpSimv_1.Visibility == Visibility.Visible;
            }
            set
            {
                if (value) RpSimv_1.Visibility = Visibility.Visible;
                else RpSimv_1.Visibility = Visibility.Hidden;
            }
        }
        private bool MinusIcon
        {
            get
            {
                return RpSimv_2.Visibility == Visibility.Visible;
            }
            set
            {
                if (value) RpSimv_2.Visibility = Visibility.Visible;
                else RpSimv_2.Visibility = Visibility.Hidden;
            }
        }
        private bool EqualIcon
        {
            get
            {
                return RpSimv_3.Visibility == Visibility.Visible;
            }
            set
            {
                if (value) RpSimv_3.Visibility = Visibility.Visible;
                else RpSimv_3.Visibility = Visibility.Hidden;
            }
        }

        private class MyCollection(DataRow row, Dictionary<long, WarehouseClass> warehouseHub) : INotifyPropertyChanged
        {
            private int _Piece = 0;
            private double _Selected_Cubature = 0;
            private OperatonEnum _IsOperation = OperatonEnum.vsPlus;
            private double _Result = 0;

            public Model.Products Products = new(row, warehouseHub);
            public event PropertyChangedEventHandler? PropertyChanged;


            public string NameItem { get => Products.NameItem; }
            public string TypeWood { get => Products.TypeWood.ToString(); }
            public string Cubature { get => Math.Round(Products.Cubature, 3).ToString(); }

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
                get => Math.Round(_Result, 2);
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
                return new Model.DocElement.Pseudonym()
                {
                    SelectedCub = Selected_Cubature,
                    Product = Products,
                    Operation = IsOperation,
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
                else if (IsOperation == OperatonEnum.vsMutation)
                {
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

        public PageRemProduct() => InitializeComponent();

        private void Загрузка(object sender, RoutedEventArgs e)
        {
            
            List<string> ls =  [nameof(DocCode.Просмотор_остатков), 
                                nameof(DocCode.Перемещение_По_Складам), 
                                nameof(DocCode.Продажи),    
                                nameof(DocCode.Списание_Продукции), 
                                nameof(DocCode.Выравнивание_Остатков)];
           
            ComboBox_Doc.ItemsSource = ls.Select(x=> x = x.Replace("_", " "));
            ComboBox_Doc.SelectedIndex = 0;// Поумолчанию просмотро документа

            if (MDL.SqlProfile is null)
            {
                //Нет подключения к БД. Проверте настройки
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
            
        }

        private void ВыбраннаСтрока(object sender, SelectionChangedEventArgs e)
        {            
            if (ComboBox_Doc.SelectedValue == null) return;
            switch (DocCode.ToCode(ComboBox_Doc.SelectedValue.ToString()!))
            {
                case DocCode.Выравнивание_Остатков:
                    PlusIcon = true;
                    MinusIcon = true;
                    EqualIcon = true;
                    SelectedDoc = () =>
                    {
                        if (First_Const(out List<GrupObj> tabel))
                        {
                            var page = new Pages.Doc.PageShipments(new XML.DocShipments() { DocTitle = "Выравнивание Остатков", Number = MDL.MyDataBase.NumberDef + 1, MainTabel = tabel }, Close_action);
                            page.Title = "Выравнивание остатков (новый)" ;
                            MDL.Organizer_Frame_Add(page);
                        }
                    };
                    break;
                case DocCode.Перемещение_По_Складам:
                    PlusIcon = true;
                    MinusIcon = true;
                    EqualIcon = false;
                    SelectedDoc = () =>
                    {                     
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

                        if (MainComboBox.SelectedItem is WarehouseClass warehouse)
                        {
                            if (First_Const(out List<GrupObj> tabel))
                            {
                                var page = new Pages.Doc.PageShipments(new XML.DocMoving() { DocTitle = "Со склада на склад", Number = MDL.MyDataBase.NumberDef + 1, MainTabel = tabel, Warehouse_From = warehouse }, Close_action)
                                {
                                    Title = "Со склада на склад (новый)"
                                };
                                MDL.Organizer_Frame_Add(page);
                            }
                        }
                    };
                    break;
                case DocCode.Продажи:
                    PlusIcon = false;
                    MinusIcon = true;
                    EqualIcon = false;
                    SelectedDoc = () =>
                    {
                        if (First_Const(out List<GrupObj> tabel))
                        {
                            var page = new Pages.Doc.PageShipments(new XML.DocSales()
                            {
                                Warehouse_From = (WarehouseClass)MainComboBox.SelectedItem,
                                DocTitle = "Продажа продукции",
                                Number = MDL.MyDataBase.NumberDef + 1,
                                MainTabel = tabel
                            }, Close_action)
                            {
                                Title = "Продажа продукции"
                            };
                            MDL.Organizer_Frame_Add(page);
                        }
                    };
                    break;
                case DocCode.Списание_Продукции:
                    PlusIcon = false;
                    MinusIcon = true;
                    EqualIcon = false;
                    SelectedDoc = () =>
                    {
                        if (First_Const(out List<GrupObj> tabel))
                        {
                            var page = new Pages.Doc.PageShipments(new XML.DocWriteDowns() {
                                Warehouse = (WarehouseClass)MainComboBox.SelectedItem,
                                DocTitle = "Списание товара", 
                                Number = MDL.MyDataBase.NumberDef + 1, 
                                MainTabel = tabel }, Close_action);
                            page.Title = "Списание товара (новый)";
                            MDL.Organizer_Frame_Add(page);
                        }
                    };
                    break;
                default:
                    PlusIcon = true;
                    MinusIcon = true;
                    EqualIcon = true;
                    SelectedDoc = () =>
                    {
                        MessageBox.Show("Выберете конкретный документ");
                    };
                    break;
            }

            for (var i = (Db_selected.Count - 1); i >= 0; i--)
            {
                if (Db_selected[i].IsOperation == OperatonEnum.vsMunis & MinusIcon == false) goto activ;
                else if (Db_selected[i].IsOperation == OperatonEnum.vsPlus & PlusIcon == false) goto activ;
                else if (Db_selected[i].IsOperation == OperatonEnum.vsMutation & EqualIcon == false) goto activ;
                continue;
            activ:;
                Db_selected[i].Selected_Cubature = 0;
                Db_selected[i].IsOperation = OperatonEnum.vsNone;
            }

            Refreh_Info();
        }

        private void Refreh_Info() {
            L_SelectRow.Content = Db_selected.Count;
            L_AllCub.Content = Db_selected.Sum(x=>x.Selected_Cubature);
        }

        /// <summary>
        /// Выполнить после закрытия формы к примеру действия обновить таблицу
        /// </summary>
        private void Close_action()
        {
            MDL.Organizer_Frame_Delete();
            ProdData = SqlRequest.GetDataTable(nameof(Products)); // обновить данные
            ВыбраннаСтрокаДляСклада(null!, null!);
        }

        /// <summary>
        /// Начальная сборка документа
        /// </summary>
        /// <param name="grups">List<GrupObj></param>
        /// <returns>true - без ошибок</returns>
        private bool First_Const(out List<GrupObj> grups)
        {
            grups = [];

            foreach (var item in Db_selected)
            {
                if (item.IsOperation == OperatonEnum.vsMunis && (item.Products.Cubature - item.Selected_Cubature) < 0)
                {
                    MessageBox.Show($"{item.Products.NameItem} не может быть меньше нуля. Отрицательный остаток не допускается");
                    return false;
                }
            }

            foreach (var item in Db_selected)
            {
                if ((item.IsOperation == OperatonEnum.vsPlus && !PlusIcon) ||
                    (item.IsOperation == OperatonEnum.vsMunis && !MinusIcon) ||
                    (item.IsOperation == OperatonEnum.vsMutation && !EqualIcon))
                {
                    continue;
                }

                var group = grups.FirstOrDefault(x => x.NameGrup == item.Products.Warehouse.NameWarehouse);
                if (group == null)
                {
                    group = new GrupObj { NameGrup = item.Products.Warehouse.NameWarehouse };
                    grups.Add(group);
                }
                group.Tabels.Add(item.ToPseudonym());
            }

            return true;
        }

        private void ВыбраннаСтрокаДляСклада(object sender, SelectionChangedEventArgs e)
        {
            if (MainComboBox.SelectedItem is not WarehouseClass warehouse) return;

            string id = warehouse.ID.ToString();
            _db.Clear();
            ProdData = SqlRequest.GetDataTable(nameof(Products), "*", "Warehouse='" + id + "'"); // загрузить сюда весь список Products
            if (ProdData is null) return;

            foreach (DataRow tv in ProdData.Rows) _db.Add(new MyCollection(tv, WarehouseHub));
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

        private void ОбновитьТекстКубах(object sender, RoutedEventArgs e)
        {             
            string str = MDL.Calculator((TextBox)sender).Replace('.', ',');
            if (double.TryParse(str, out double d))
            {
                ((MyCollection)DG_Main.SelectedItem).Selected_Cubature = d;
            }
            Refreh_Info();
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

        private void ТекстОбновленКоличест(object sender, TextChangedEventArgs e)
        {
            if (DG_Main.SelectedItem is MyCollection my)
            {
                if (int.TryParse(MDL.Calculator((TextBox)sender), out int i))
                    my.Piece = i;
            }
        }

        private void СоздатьДокумент(object sender, RoutedEventArgs e)
        {
            SelectedDoc?.Invoke();
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
                        if (MDL.SqlProfile.GetFieldSql(my.Products.ID, nameof(Model.Products)).Length == 0) _db.Remove(my);
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

        private void DG_Main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
