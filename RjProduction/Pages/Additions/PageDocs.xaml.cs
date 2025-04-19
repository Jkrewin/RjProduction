using RjProduction.Model;
using RjProduction.WpfFrm;
using RjProduction.XML;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace RjProduction.Pages.Additions
{
    public partial class PageDocs : Page
    {

        private const string DEL_SIMBOL = "";
        private readonly ObservableCollection<MyItemProd> _db = [];
        private string? TabelName;
        private DataTable? _TabelData;
        private Func<long,IDocMain>? GetFuncDoc;
        private bool _DateFilter ;
        private bool DateFilter { get => _DateFilter; set { _DateFilter = value; if (value) FilterButton.Visibility = Visibility.Visible; else FilterButton.Visibility = Visibility.Hidden; } }

        public PageDocs()
        {
            InitializeComponent();
        }


        private class MyItemProd : INotifyPropertyChanged
        {
            private double _cubs;
            private string _isDel = " ";

            public event PropertyChangedEventHandler? PropertyChanged;
            public string NameItem { get; set; }
            public string DateDoc { get; set; }
            public string Num { get; set; }
            public string Summ { get; set; }
            public string Cubs
            {
                get => Math.Round(_cubs, 2).ToString();
                set
                {
                    if (double.TryParse(value, out double d))
                    {
                        _cubs = d;
                    }
                    else _cubs = 0;
                }
            }
            public long ID;
            public string IsDel
            {
                get => _isDel;
                set { _isDel = value; NotifyPropertyChanged(); }
            }

            public MyItemProd(DataRow dataRow)
            {
                if (long.TryParse(dataRow[nameof(Sql.SqlParam.ID)].ToString(), out long l)) ID = l;
                NameItem = dataRow[nameof(IDocMain.DocTitle)].ToString() ?? "NaN";
                DateDoc = DateOnly.Parse(dataRow[nameof(IDocMain.DataCreate)].ToString() ?? "01.01.2000").ToString("dd.MM.yyyy");
                Num = dataRow[nameof(IDocMain.Number)].ToString() ?? "NaN";
                Summ = dataRow["Amount"].ToString() ?? "NaN";
                Cubs = dataRow["Cubs"].ToString() ?? "NaN";               
                var s =  dataRow[nameof(Sql.SqlParam.ActiveObjIsDelete)].ToString() ?? "0";
                IsDel = (s == "0" ? "" : DEL_SIMBOL);
            }

            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

       

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            DateFilter = false;

            List<DeliveredStruct> d=[];
            TabelName = "";

            // Типы документов создаються тут
            d.Add(new DeliveredStruct("Поступление (производство)", 0,"", () => {
                TabelName = nameof(DocArrival);
                Refreh__TabelData();
                GetFuncDoc = (id) => Sql.SqlRequest.ReadData<DocArrival>(id);
            } ));

            d.Add(new DeliveredStruct("Выравнивание остатка", 0, "", () => {
                TabelName = nameof(DocShipments);
                Refreh__TabelData();
                GetFuncDoc = (id) => Sql.SqlRequest.ReadData<DocShipments>(id);
            }));

            d.Add(new DeliveredStruct("Со склада на склад", 0, "", () => {
                TabelName = nameof(DocMoving);
                Refreh__TabelData();
                GetFuncDoc = (id) => Sql.SqlRequest.ReadData<DocMoving>(id);
            }));

            d.Add(new DeliveredStruct("Списание продукции", 0, "", () => {
                TabelName = nameof(DocWriteDowns);
                Refreh__TabelData();
                GetFuncDoc = (id) => Sql.SqlRequest.ReadData<DocWriteDowns>(id);
            }));

            MainComboBox.DisplayMemberPath = "Name";
            MainComboBox.ItemsSource = d;
            DG_Main.ItemsSource = _db;
        }

        private void Refreh__TabelData()
        {
            if (string.IsNullOrEmpty(TabelName)) return;

            if (DateFilter ==false)
            {
                _TabelData = Sql.SqlRequest.GetDataTable(TabelName);
            }
            else
            {
                string d1 = Date1.SelectedDate!.Value.ToString("yyyy-M-d");
                string d2 = Date2.SelectedDate!.Value.ToString("yyyy-M-d");
                _TabelData = Sql.SqlRequest.GetDataTable(TabelName, "*", $"{nameof(IDocMain.DataCreate)}>='{d1}' AND {nameof(IDocMain.DataCreate)}<'{d2}'");
            }
            Refreh_Tabel();
        }

        public void Refreh_Tabel()
        {
            if (_TabelData is null) return;
            _db.Clear();
            foreach (DataRow item in _TabelData.Rows)
            {
                _db.Add(new MyItemProd(item));
            }
            DG_Main.Items.Refresh();
        }

        private void ВыбраннаСтрока(object sender, SelectionChangedEventArgs e)
        {
             if (MainComboBox.SelectedValue is DeliveredStruct delivered) {
                if (delivered.GetAct is not null) delivered.GetAct();
                else MessageBox.Show($"0054# Ошибка {delivered.Name}, при чтении данных id={delivered.ID}");
            }            
        }

        private void ПометитьНаУдаление(object sender, RoutedEventArgs e)
        {
            if (TabelName is null) return;
            MyItemProd? item = DG_Main.SelectedItem as MyItemProd;
            if (item is not null)
            {
                if (item.IsDel == DEL_SIMBOL)
                {
                    item.IsDel = DEL_SIMBOL;
                    Sql.SqlRequest.Mark_ActiveObjIsDelete(item.ID, TabelName, false);
                }
                else
                {
                    item.IsDel = DEL_SIMBOL;
                    Sql.SqlRequest.Mark_ActiveObjIsDelete(item.ID, TabelName);
                }
            }
        }

        private void ОткрытьСведения(object sender, RoutedEventArgs e) => ОткрытьСведения(null!, null!);

        private void ОткрытьСведения(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TabelName is null) return;
            if (DG_Main.SelectedItem is MyItemProd item)
            {
                IDocMain tv;
                if (GetFuncDoc is null) return;
                tv = GetFuncDoc(item.ID);
                var tv2 = new Additions.PageViewDoc(tv);
                var w = new WpfFrm.WpfView(tv2, tv.Number + "/" + tv.DataCreate,
                    () => { });
                w.Show();
            }
        }

        private void ФильтерДаты(object sender, RoutedEventArgs e)
        {
            
            if (Date1.SelectedDate.HasValue == false | Date2.SelectedDate.HasValue == false)
            {
                MessageBox.Show("Укажите даты для поиска");
            }
            else
            {
                DateFilter = true;
                Refreh__TabelData();
            }
        }

        private void ФильтерДатыУдалить(object sender, RoutedEventArgs e)
        {
            DateFilter = false;
            Refreh__TabelData();
        }
    }
}
