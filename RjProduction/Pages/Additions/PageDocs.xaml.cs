using RjProduction.XML;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace RjProduction.Pages.Additions
{
    public partial class PageDocs : Page
    {
        private readonly List<MyItem> _db = [];
        private DataTable? _TabelData;
        public PageDocs()
        {
            InitializeComponent();
        }

        private class MyItem
        {
            public string NameItem { get; set; }
            public string DateDoc { get; set; }
            public string Num { get; set; }
            public string Summ { get; set; }
            public string Cubs { get; set; }
            public long ID;

            public MyItem(DataRow dataRow)
            {
                if (long.TryParse(dataRow["ID"].ToString(), out long l)) ID = l;
                NameItem = dataRow["DocTitle"].ToString() ?? "NaN";
                DateDoc = dataRow["DataCreate"].ToString() ?? "NaN";
                Num = dataRow["Number"].ToString() ?? "NaN";
                Summ = "NaN";
                Cubs = "NaN";
            }
        }


        private void Загруженно(object sender, RoutedEventArgs e)
        {
            MainComboBox.ItemsSource = Model.DocCode.ToArray();
            DG_Main.ItemsSource = _db;

        }

        public void Refreh_Tabel()
        {
            if (_TabelData is null) return;
            _db.Clear();
            foreach (DataRow item in _TabelData.Rows)
            {
                _db.Add(new MyItem(item));
            }
            DG_Main.Items.Refresh();
        }

        private void ВыбраннаСтрока(object sender, SelectionChangedEventArgs e)
        {
            switch (MainComboBox.SelectedValue.ToString())
            {
                case nameof(Model.DocCode.Производство_склад):
                     _TabelData = Sql.SqlRequest.GetDataTable(nameof(DocArrival));    
                    break;
                case nameof(Model.DocCode.ВыравниваниеОстатков):
                    _TabelData = Sql.SqlRequest.GetDataTable(nameof(DocShipments));
                    break;
                default:
                    break;
            }
            Refreh_Tabel();
        }

        private void ВыборОбъекта(object sender, SelectedCellsChangedEventArgs e)
        {

        }
    }
}
