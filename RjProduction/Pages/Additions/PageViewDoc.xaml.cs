using RjProduction.Model;
using RjProduction.XML;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using RjProduction.Model.DocElement;

namespace RjProduction.Pages.Additions
{
    public partial class PageViewDoc : Page
    {
        const string SELECT_CNTR = "#FFF8DAC1";
        const string ON_SELECT_CNTR = "#FFF0F3EF";

        private readonly IDocMain Doc;
        private List<DocRow> DocRows = [];
        private string[]? _name_col;


        public PageViewDoc(IDocMain docMain )
        {
            InitializeComponent();
            Doc = docMain;           
        }

        private void Печать_документа(object sender, RoutedEventArgs e)
        {
            if (Doc is DocArrival arrival)
            {
                Report_Накладная("Производство", arrival.Warehouse.NameWarehouse + " " + arrival.Warehouse.AddressWarehouse);
            }
            else if (Doc is DocMoving moving)
            {
                Report_Накладная(moving.Warehouse_From.NameWarehouse + " " + moving.Warehouse_From.AddressWarehouse, moving.Warehouse_To!.NameWarehouse + " " + moving.Warehouse_To!.AddressWarehouse);
            }
            else if (Doc is DocShipments)
            {
                Report_Накладная("Выравнивание остатков", "");
            }
        }

        private void Report_Накладная(string toOut, string toIn) {
            var report = new HtmlReportDiv(AppDomain.CurrentDomain.BaseDirectory + @"Res\Div\Накладная.htm");
            decimal dec = 0;

            HtmlReportDiv.SetValue[] arr = [
                new HtmlReportDiv.SetValue("numer", Doc.Number.ToString()),
                new HtmlReportDiv.SetValue("date", Doc.DataCreate.ToString()),
                new HtmlReportDiv.SetValue("To_out", toOut),
                new HtmlReportDiv.SetValue("To_in", toIn)
            ];
            string header = report.GetDiv("header", arr).Replace("</table>", "");
            string final = report.GetDiv("final") + "</table>";
            string deep = "";

            for (int i = 0; i < DocRows.Count; i++)
            {
                if (DocRows[i].TypeObj == DocRow.КруглыйЛес | DocRows[i].TypeObj == DocRow.Пиломатериалы)
                {
                    HtmlReportDiv.SetValue[] d = [
                            new HtmlReportDiv.SetValue("#", (i+1).ToString()),
                            new HtmlReportDiv.SetValue("Наименование",DocRows[i].NameObj),
                            new HtmlReportDiv.SetValue("Ед","М3"),
                            new HtmlReportDiv.SetValue("Количество",DocRows[i].CubatureAll),
                            new HtmlReportDiv.SetValue("Цена",DocRows[i].Price),
                            new HtmlReportDiv.SetValue("Сумма",DocRows[i].Amount)  ];
                    dec += DocRows[i].Amount;
                    deep += report.GetDiv("cell", d);
                }
            }

            string end = report.GetDiv("end_table", [new HtmlReportDiv.SetValue("Итог", dec)]) + "</table>";

            HtmlReportDiv.OpenReport(header + deep + end + final);
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            Doc.MainTabel.Add(new GrupObj() { NameGrup = "main" });
            DocRows = Sql.SqlRequest.ReadСollection<DocRow>( $"{nameof(DocRow.ID_Doc)}='{Doc.ID_Doc}'") ?? [];           
            Doc.MainTabel[0].Tabels = [.. DocRows];

            MDL.ImportToWpf(this, Doc);
            Title = Doc.DataCreate.ToString();

            ВыборГруппы(ButtonFirst, null!);
        }

        private void VisibleColumns(string name_Header, Visibility visibility)
        {
            _name_col ??= MainDataGrid.Columns.Select(x => x.Header.ToString() ?? string.Empty).ToArray<string>();

            for (int i = 0; i < _name_col.Length; i++)
            {
                if (_name_col[i] == name_Header) MainDataGrid.Columns[i].Visibility = visibility;
            }

        }
        private void RenameColumns(string name_Header, string newName)
        {
            _name_col ??= [.. MainDataGrid.Columns.Select(x => x.Header.ToString() ?? string.Empty)];
            for (int i = 0; i < _name_col.Length; i++)
            {
                if (_name_col[i] == name_Header)
                {
                    MainDataGrid.Columns[i].Header = newName;
                }
            }
        }

        private void ВыборГруппы(object sender, RoutedEventArgs e)
        {
            foreach (var item in SP_Buttons.Children)
            {
                if (item is Button button)
                {
                    button.Background = MDL.BrushConv(ON_SELECT_CNTR);
                }
            }

            Button b = (Button)sender;
            b.Background = MDL.BrushConv(SELECT_CNTR);

            var result = DocRows.Where(x => b.Uid == x.TypeObj).ToList();
            if (result.Count == 0)
            {
                MainDataGrid.ItemsSource = new List<DocRow>();

            }
            else  MainDataGrid.ItemsSource = result; 

            if (b.Uid == DocRow.КруглыйЛес || b.Uid == DocRow.Пиломатериалы)
            {
                VisibleColumns("ID", Visibility.Collapsed);
                VisibleColumns("ID_Doc", Visibility.Collapsed);
                VisibleColumns("TypeObj", Visibility.Collapsed);
                VisibleColumns("Comment", Visibility.Collapsed);
                VisibleColumns("UpRaise", Visibility.Collapsed);
                VisibleColumns("ActiveObjIsDelete", Visibility.Collapsed);
                VisibleColumns("LockInfo", Visibility.Collapsed);
                VisibleColumns("GrupName", Visibility.Collapsed);
                VisibleColumns("TabelName", Visibility.Collapsed);

                RenameColumns("NameObj", "Наименование");
                RenameColumns("Amount", "Сумма");
                RenameColumns("Quantity", "Количество");
                RenameColumns("CubatureAll", "Куботура");
                RenameColumns("Price", "Цена");
            }
            else if (b.Uid == DocRow.Сотрудники)
            {
                VisibleColumns("ID", Visibility.Collapsed);
                VisibleColumns("ID_Doc", Visibility.Collapsed);
                VisibleColumns("TypeObj", Visibility.Collapsed);
                VisibleColumns("Comment", Visibility.Collapsed);
                VisibleColumns("UpRaise", Visibility.Collapsed);
                VisibleColumns("ActiveObjIsDelete", Visibility.Collapsed);
                VisibleColumns("LockInfo", Visibility.Collapsed);
                VisibleColumns("GrupName", Visibility.Collapsed);
                VisibleColumns("TabelName", Visibility.Collapsed);
                VisibleColumns("Quantity", Visibility.Collapsed);
                VisibleColumns("CubatureAll", Visibility.Collapsed);
                VisibleColumns("Price", Visibility.Collapsed);

                RenameColumns("NameObj", "Сотрудник");
                RenameColumns("Amount", "Оплата");
                RenameColumns("Comment", "");
            }


        }
    }
}
