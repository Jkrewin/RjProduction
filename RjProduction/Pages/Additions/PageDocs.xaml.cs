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

        public PageDocs()
        {
            InitializeComponent();
        }


        private class MyItemProd : INotifyPropertyChanged
        {
            private double _cubs;
            private string _isDel=" ";
            
            public event PropertyChangedEventHandler? PropertyChanged;
            public string NameItem { get; set; }
            public string DateDoc { get; set; }
            public string Num { get; set; }
            public string Summ { get; set; }
            public string Cubs { get => Math.Round(_cubs,2).ToString(); 
                set {
                    if (double.TryParse(value, out double d))
                    {
                        _cubs = d;
                    }
                    else _cubs = 0;                
                } }
            public long ID;
            public string IsDel { 
                get=>_isDel; 
                set { _isDel = value; NotifyPropertyChanged(); } 
            }

            public MyItemProd(DataRow dataRow)
            {
                if (long.TryParse(dataRow["ID"].ToString(), out long l)) ID = l;
                NameItem = dataRow["DocTitle"].ToString() ?? "NaN";
                DateDoc = dataRow["DataCreate"].ToString() ?? "NaN";
                Num = dataRow["Number"].ToString() ?? "NaN";
                Summ = dataRow["Amount"].ToString() ?? "NaN";
                Cubs = dataRow["Cubs"].ToString() ?? "NaN";               
                var s =  dataRow[nameof(Sql.SqlParam.ActiveObjIsDelete)].ToString() ?? "0";
                IsDel = (s == "0" ? "" : DEL_SIMBOL);
            }

            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

       

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            List<DeliveredStruct> d=[];
            TabelName = "";

            d.Add(new DeliveredStruct("Поступление (производство)", 0,"", () => {
                TabelName = nameof(DocArrival);
                _TabelData = Sql.SqlRequest.GetDataTable(nameof(DocArrival));
                GetFuncDoc = (id) => Sql.SqlRequest.ReadData<DocArrival>(id);
            } ));

            d.Add(new DeliveredStruct("Выравнивание остатка", 0, "", () => {
                TabelName = nameof(DocShipments);
                _TabelData = Sql.SqlRequest.GetDataTable(nameof(DocShipments));
                GetFuncDoc = (id) => Sql.SqlRequest.ReadData<DocShipments>(id);
            }));

            d.Add(new DeliveredStruct("Со склада на склад", 0, "", () => {
                TabelName = nameof(DocMoving);
                _TabelData = Sql.SqlRequest.GetDataTable(nameof(DocMoving));
                GetFuncDoc = (id) => Sql.SqlRequest.ReadData<DocMoving>(id);
            }));

            MainComboBox.DisplayMemberPath = "Name";
            MainComboBox.ItemsSource = d;
            DG_Main.ItemsSource = _db;
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
            Refreh_Tabel();
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
    }
}
