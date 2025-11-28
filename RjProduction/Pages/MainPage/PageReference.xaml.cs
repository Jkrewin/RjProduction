using System.Collections;
using RjProduction.Model.DocElement;
using System.Windows;
using System.Windows.Controls;
using RjProduction.Model;

namespace RjProduction.Pages
{
    public partial class PageReference : Page
    {
        private IList? _list;
        private MainRow? DelObj;
        private object? EndEdit;

        public PageReference()
        {
            InitializeComponent();
        }
        /// <summary>
        /// общий шаблон
        /// </summary>
        private abstract class MainRow
        {
            public required object Obj;
            public abstract void RefrehData();
            public abstract void CreateNew();
        }
        /// <summary>
        /// для материалов
        /// </summary>
        private class RowMaterial : MainRow
        {
            public required float Высота { get; set; }
            public required float Ширина { get; set; }
            public required float Длинна { get; set; }
            public required double Цена { get; set; }
            public required string Общее_Название { get; set; }

            public override void CreateNew()
            {
                MDL.MyDataBase.MaterialsDic.Add(new MaterialObj());
                Obj = MDL.MyDataBase.MaterialsDic[^1];
            }

            public override void RefrehData()
            {
                var index = MDL.MyDataBase.MaterialsDic.FindIndex(x => x == ((MaterialObj)Obj));
                if (index == -1) return;
                var t = MDL.MyDataBase.MaterialsDic[index];
                t.WidthMaterial = Ширина;
                t.HeightMaterial = Высота;
                t.LongMaterial = Длинна;
                t.Price = Цена;
                Obj = t;
                MDL.MyDataBase.MaterialsDic[index] = t;
            }
        }
        /// <summary>
        /// для сотрудников
        /// </summary>
        private class RowEmpl : MainRow
        {
            public required Employee ФИО { get; set; }

            public override void CreateNew()
            {
                MDL.MyDataBase.EmployeeDic.Add(ФИО);
                Obj = MDL.MyDataBase.EmployeeDic[^1];
            }

            public override void RefrehData()
            {
                var index = MDL.MyDataBase.EmployeeDic.FindIndex(x => x.Equals((string)Obj));
                MDL.MyDataBase.EmployeeDic[index] = ФИО;
            }
        }
        /// <summary>
        /// для групп
        /// </summary>
        private class GrupName : MainRow
        {
            public required string НазваниеГруппы { get; set; }

            public override void CreateNew()
            {
                MDL.MyDataBase.NamesGrup.Add(НазваниеГруппы);
                Obj = MDL.MyDataBase.NamesGrup[^1];
            }

            public override void RefrehData()
            {
                var index = MDL.MyDataBase.NamesGrup.FindIndex(x => x.Equals((string)Obj));
                MDL.MyDataBase.NamesGrup[index] = НазваниеГруппы;
            }
        }
        /// <summary>
        /// для групп
        /// </summary>
        private class WarehouseEdit : MainRow
        {
            public required string НазваниеСклада { get; set; }
            public required string Описание { get; set; }
            public required string Адрес { get; set; }

            public override void CreateNew()
            {
               // Нужна форма для добавления
            }

            public override void RefrehData()
            {
                // Нужна форма для добавления
            }
        }

        private void ClearUI() {
            Button_add_edit.Visibility = Visibility.Hidden;
            DG_Main.CanUserAddRows = true;
            DG_Main.SelectionUnit = DataGridSelectionUnit.Cell;
            DG_Main.IsReadOnly = false;
        }

        private void УдаляетСтроку(object sender, RoutedEventArgs e)
        {
            if (DelObj != null)
            {
                if (_list is null)
                {
                    MDL.LogError("Удаление строки невозвожно Справочник не загружен ");
                    return;
                }
                _list.Remove(DelObj.Obj);
                ((IList)DG_Main.ItemsSource).Remove(DelObj);
                DG_Main.Items.Refresh();

                if (DelObj.Obj is WarehouseClass warehouse ) {
                    if (warehouse.ID == -1) return;
                    if (MessageBox.Show("Удалить этот склад из общей базы данных ? Важно, на этом складе продукция или товар должен быть нулевой", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                        warehouse.Delete();
                    } 
                }

                DelObj = null;
            }
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            DG_Main.CellEditEnding += ИзмененияВнесены;
            DG_Main.CurrentCellChanged += ЯчекаИзменена;
           
        }      

        private void ВыборОбъекта(object sender, SelectedCellsChangedEventArgs e)
        {
            if (e.AddedCells.Count != 0) if (e.AddedCells[0].Item is MainRow row) DelObj = row;
        }

        public void МоиСотрудники(object sender, RoutedEventArgs e)
        {
            ClearUI();
            List<RowEmpl> ls = [];
            foreach (var item in MDL.MyDataBase.EmployeeDic)
            {
                ls.Add(new RowEmpl()
                {
                    Obj = item,
                    ФИО = item
                });
            }
            DG_Main.ItemsSource = ls;
            _list = MDL.MyDataBase.EmployeeDic;
            DG_Main.Columns[0].Width = 250;
        }

        public void МоиМатериалы(object sender, RoutedEventArgs e)
        {
            ClearUI();
            List<RowMaterial> ls = [];
            foreach (var item in MDL.MyDataBase.MaterialsDic)
            {
                ls.Add(new RowMaterial()
                {
                    Obj = item,
                    Высота = item.HeightMaterial,
                    Длинна = item.LongMaterial,
                    Цена = item.Price,
                    Ширина = item.WidthMaterial,
                    Общее_Название = item.NameMaterial
                    
                });
            }
            DG_Main.ItemsSource = ls;
            _list = MDL.MyDataBase.MaterialsDic;
            DG_Main.Columns[0].Width = 60;
            DG_Main.Columns[1].Width = 60;
            DG_Main.Columns[2].Width = 60;
            DG_Main.Columns[3].Width = 80;
            DG_Main.Columns[4].Width = 180;
        }
              
        private void ЯчекаИзменена(object? sender, EventArgs e)
        {
            if (EndEdit is MainRow row)
            {
                if (row.Obj == null) row.CreateNew();
                row.RefrehData();
            }
        }

        private void ИзмененияВнесены(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit) EndEdit = e.Row.Item;
        }

        public void МоиГруппы(object sender, RoutedEventArgs e)
        {
            ClearUI();
            List<GrupName> ls = [];
            foreach (var item in MDL.MyDataBase.NamesGrup)
            {
                ls.Add(new GrupName()
                {
                    Obj = item,
                    НазваниеГруппы = item
                });
            }
            DG_Main.ItemsSource = ls;
            _list = MDL.MyDataBase.NamesGrup;
            DG_Main.Columns[0].Width = 250;
        }

        private void ВыгрузкаИзПамяти(object sender, RoutedEventArgs e)=> MDL.SaveXml<MDL.Reference>(MDL.MyDataBase, MDL.SFile_DB);

        public void МоиСклады() {
            DG_Main.SelectionUnit = DataGridSelectionUnit.FullRow;
            DG_Main.IsReadOnly = true;
            List<WarehouseEdit> ls = [];
            foreach (var item in MDL.MyDataBase.Warehouses) {
                ls.Add(new WarehouseEdit() { Obj =item,
                    Адрес = item.AddressWarehouse, 
                    НазваниеСклада = item.NameWarehouse ,
                    Описание = item.DescriptionWarehouse
                });
            }

            DG_Main.ItemsSource = ls;
            Button_add_edit.Visibility = Visibility.Visible;
            _list = MDL.MyDataBase.Warehouses;
            DG_Main.CanUserAddRows = false;
            
            DG_Main.Columns[0].Header = "Название склада";
            DG_Main.Columns[1].Header = "Описание склада";
            DG_Main.Columns[2].Header = "Адрес склада       ";
        }

        public void МоиАдреса() { 
        
        }

        private void Добавить_изменить(object sender, RoutedEventArgs e)
        {            
           
                // Редактор складов
                var wpf = new WpfFrm.WpfWarehouse((w) =>
                {

                    МоиСклады();

                    if (w is null) return;
                    for (global::System.Int32 i = 0; i < MDL.MyDataBase.Warehouses.Count; i++)
                    {
                        if (w.Equals(MDL.MyDataBase.Warehouses[i]))
                        {
                            DG_Main.SelectedIndex = i;
                            break;
                        }
                    }
                }
                );
                wpf.ShowDialog();
            

        }

       
    }
}
