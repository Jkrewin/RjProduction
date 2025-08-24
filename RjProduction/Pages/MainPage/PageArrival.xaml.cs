using RjProduction.Model;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using RjProduction.XML;
using RjProduction.Pages.Doc;
using RjProduction.Model.DocElement;

namespace RjProduction.Pages
{
    public partial class PageArrival : Page
    {
        const int START_DATE = 2000;

        private List<IDocMain> Docs = [];
        private DateOnly? SelectDate = null;
        private string ItemStringDoc = string.Empty;

        public PageArrival()
        {
            InitializeComponent();
        }

        private void Загрузка(object sender, RoutedEventArgs e)
        {
            MainComboBox.ItemsSource = Model.DocCode.ToArray();
            MainComboBox.SelectedIndex = 1;
            // Перехват нажатий
            Window.GetWindow(this).KeyDown += (object sender, KeyEventArgs e) => MDL.HandleKey?.HandleKeyPress(sender, e);

            List<int> ints = [];
            for (int i = START_DATE; i < 2100; i++)
            {
                ints.Add(i);
            }
            MonYears.ItemsSource = ints;

        }

      
        #region "Ридер строк таблици по UI"
        private DataGridCell? GetCell(int row, int column)
        {
            DataGridRow? rowContainer = GetRow(row);
            if (rowContainer != null)
            {
                DataGridCellsPresenter? presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter == null){
                    DataG_Main.ScrollIntoView(rowContainer, DataG_Main.Columns[column]);
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
            DataGridRow row = (DataGridRow)DataG_Main.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                DataG_Main.UpdateLayout();
                DataG_Main.ScrollIntoView(DataG_Main.Items[index]);
                row = (DataGridRow)DataG_Main.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }
        #endregion

        private void Refreh_DataG_Main()
        {
            if (Ration_now.IsChecked == true) { Docs = MDL.GetDocuments(ItemStringDoc) ?? []; }
            else if (SelectDate is not null)
            {
                Docs = MDL.GetDocuments(SelectDate.Value.Year, SelectDate.Value.Month, ItemStringDoc) ?? [];
            }
            else return;
            DataG_Main.ItemsSource = Docs;
            for (int i = 5; i < DataG_Main.Columns.Count; i++) DataG_Main.Columns[i].Visibility = Visibility.Collapsed;

            const int StatusColums = 3; // колонка м кнопкой         
            for (global::System.Int32 i = 0; i < Docs.Count; i++)
            {
                var cell = GetCell(i, StatusColums);
                if (cell is not null)
                {
                    if (GetVisualChild<Button>(cell) is Button b) SwitchButton(b, Docs[i]);
                }
            }
        }

        private static void SwitchButton(Button b, IDocMain doc) {
            switch (doc.Status)
            {
                case StatusEnum.Не_Проведен:
                    b.Background = MDL.BrushConv("#FF797979");
                    b.Content = "Не Проведен";
                    break;
                case StatusEnum.Проведен:
                    b.Background = MDL.BrushConv("#FF48C0DE");
                    b.Content = "Проведен";
                    break;
                case StatusEnum.Частично:
                    b.Background = Brushes.Brown;
                    b.Content = "Проведен частично";
                    break;
                case StatusEnum.Изменён:
                    b.Background = Brushes.Blue;
                    b.Content = "Изменения";
                    break;
                case StatusEnum.Ошибка:
                default:
                    b.Background = MDL.BrushConv("#FFDE4848");
                    b.Content = "Ошибка";
                    break;
            }
        }

       private void ДвойноеНажатие(object sender, MouseButtonEventArgs e)
        {
            if (DataG_Main.SelectedItem == null) return;

            if (((IDocMain)DataG_Main.SelectedItem).Status == StatusEnum.Ошибка) {
                ((IDocMain)DataG_Main.SelectedItem).Status = StatusEnum.Не_Проведен;
            }
          
            
            if (DataG_Main.SelectedItem is DocArrival arrival)
            {
                var page = new PageDocEditor(arrival, (Action)(() =>
                {
                    MDL.Organizer_Frame_Delete();
                    Refreh_DataG_Main();
                }));
                MDL.Organizer_Frame_Add (page);
            }
            else if (DataG_Main.SelectedItem is DocShipments shipments)
            {
                var page = new PageShipments(shipments, (Action)(() =>
                   {
                       MDL.Organizer_Frame_Delete();
                       Refreh_DataG_Main();
                   }))
                {
                    Title = shipments.DocTitle
                };
                MDL.Organizer_Frame_Add(page);
            }
            else if (DataG_Main.SelectedItem is DocMoving moving)
            {
                var page = new PageShipments(moving, (Action)(() =>
                {
                    MDL.Organizer_Frame_Delete();
                    Refreh_DataG_Main();
                }))
                {
                    Title = moving.DocTitle
                };
                MDL.Organizer_Frame_Add(page);
            }
            else throw new Exception("Отсутсвует такой тип документа");
        }

        private void ОткрытьОкноОбъектов(object sender, RoutedEventArgs e)
        {            
            var doc = new XML.DocArrival() { Number = MDL.MyDataBase.NumberDef };
            doc.MainTabel.Add(new GrupObj() { NameGrup = "Основная" });
           var page = new PageDocEditor(doc, () =>
            {
                MDL.Organizer_Frame_Delete();
                Refreh_DataG_Main();
            });
            page.Title= doc.DocTitle + " " + doc.Number;
            MDL.Organizer_Frame_Add(page);
        }

        private void ПровестиДок(object sender, RoutedEventArgs e)
        {            
            Button b = (Button)sender;
            if (DataG_Main.SelectedItem is not IDocMain document)
            {
                MessageBox.Show("Документ невозможно провести сейчас.");
                return;
            }
            document.CarryOut();
            SwitchButton(b,document);
        }

        private void УдалитьОбъект(object sender, RoutedEventArgs e)
        {
            IDocMain d = (IDocMain)DataG_Main.SelectedItem;
            string sFile = AppDomain.CurrentDomain.BaseDirectory + "xmldocs\\";
            sFile += d.DataCreate.Year.ToString() +"\\"+ d.DataCreate.Month.ToString() +"\\" +((XmlProtocol)DataG_Main.SelectedItem).FileName(d.Doc_Code);
            
            if (File.Exists(sFile))
            {
                try
                {
                    File.Delete(sFile);
                }
                catch (Exception ex)
                {
                    MDL.LogError( "Ошибка при удаление файла " + sFile,ex.Message);
                }
            }
            Refreh_DataG_Main();
        }
        
        private void ВыбраннаСтрока(object sender, SelectionChangedEventArgs e)
        {
            ItemStringDoc = DocCode.ToCode(MainComboBox.SelectedValue.ToString() ?? string.Empty);
            Refreh_DataG_Main();
        }

        private void ДобавитьФайл(object sender, RoutedEventArgs e)
        {

        }
            
        private void ПолучениеСтраници(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (((Frame)sender).Content is IKeyControl control) {
                                   MDL.HandleKey = control;       
            }
        }

        private void ВыборТекущаяДата(object sender, RoutedEventArgs e)
        {
            if (DPDataNow is null) return;
            DPDataNow.IsEnabled = false;
            SelectDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1);
            Refreh_DataG_Main();           
        }

        private void ВыборДата(object sender, RoutedEventArgs e)
        {
            DPDataNow.IsEnabled = true;
            MonYears.SelectedIndex= DateTime.Now.Year-START_DATE;
            ВыборДатыИлиГода(null!, null!);
        }

        private void ВыборДатыИлиГода(object sender, SelectionChangedEventArgs e)
        {
            if (MonYears.SelectedValue is null | MonMonf.SelectedValue is null ) return;
            SelectDate = new DateOnly(Convert.ToInt32(MonYears.SelectedValue), MonMonf.SelectedIndex + 1, 1);
            Refreh_DataG_Main();
        }
    }



}
