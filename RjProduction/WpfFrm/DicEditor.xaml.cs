using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static RjProduction.Model.Document;

namespace RjProduction.WpfFrm
{
    /// <summary>
    /// Логика взаимодействия для DicEditor.xaml
    /// </summary>
    public partial class DicEditor : Window
    {
        private IList? _list;
        private MainRow? DelObj ;
        private object? EndEdit;

        public DicEditor()
        {
            InitializeComponent();
        }

        private abstract class MainRow {
            public required object Obj;
            public abstract void RefrehData();
            public abstract void CreateNew();
        }

        private class RowMaterial: MainRow
        { 
            public required double Высота { get; set; }
            public required double Ширина { get; set; }
            public required double Длинна { get; set; }
            public required double Цена { get; set; }

            public override void CreateNew()
            {
                MDL.MyDataBase.MaterialsDic.Add(new MaterialObj());
                Obj = MDL.MyDataBase.MaterialsDic[^1];
            }

            public override void RefrehData()
            {               
                var index = MDL.MyDataBase.MaterialsDic.FindIndex(x => x == ((MaterialObj)Obj));
                var t = MDL.MyDataBase.MaterialsDic[index];
                t.WidthMaterial = Ширина;
                t.HeightMaterial = Высота;
                t.LongMaterial = Длинна;
                t.Price = Цена;
                Obj = t;
                MDL.MyDataBase.MaterialsDic[index] = t;
            }
        }

        private class RowEmpl : MainRow
        {
            public required string ФИО { get; set; }

            public override void CreateNew() {
                MDL.MyDataBase.EmployeeDic.Add(ФИО);
                Obj = MDL.MyDataBase.EmployeeDic[^1];
            }

            public override void RefrehData()
            {
                var index = MDL.MyDataBase.EmployeeDic.FindIndex(x => x.Equals((string)Obj));
                MDL.MyDataBase.EmployeeDic[index] = ФИО;
            }
        }

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


        private void Загруженно(object sender, RoutedEventArgs e)
        {
            DG_Main.CellEditEnding += ИзмененияВнесены;
            DG_Main.CurrentCellChanged += ЯчекаИзменена;
            DG_Main.AddingNewItem += НоваяЯчейка;
        }

        private void НоваяЯчейка(object? sender, AddingNewItemEventArgs e)
        {
            var tt = ((IList)DG_Main.ItemsSource)[^1];
        }

        private void ВыборОбъекта(object sender, SelectedCellsChangedEventArgs e)
        {
            if (e.AddedCells.Count != 0) if (e.AddedCells[0].Item is MainRow row) DelObj = row;
        }

        private void МоиСотрудники(object sender, RoutedEventArgs e)
        {            
            List<RowEmpl> ls = new List<RowEmpl>();
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

        private void МоиМатериалы(object sender, RoutedEventArgs e)
        {            
            List<RowMaterial> ls = new List<RowMaterial>();
            foreach (var item in MDL.MyDataBase.MaterialsDic)
            {
                ls.Add(new RowMaterial()
                {
                    Obj= item,
                    Высота = item.HeightMaterial,
                    Длинна = item.LongMaterial,
                    Цена = item.Price,
                    Ширина = item.WidthMaterial
                });
            }
            DG_Main.ItemsSource = ls;
            DG_Main.Columns[0].Width = 60;
            DG_Main.Columns[1].Width = 60;
            DG_Main.Columns[2].Width = 60;
            DG_Main.Columns[3].Width = 80;
        }

        private void УдаляетСтроку(object sender, RoutedEventArgs e)
        {
            if (DelObj != null)
            {
                _list!.Remove(DelObj.Obj);
                ((IList)DG_Main.ItemsSource).Remove(DelObj);
                DG_Main.Items.Refresh();
                DelObj = null;
            }
        }

        private void ВыходИзФормы(object sender, EventArgs e)=> MDL.SaveXml<MDL.BoardDic>(MDL.MyDataBase, MDL.SFile_DB);

        private void ЯчекаИзменена(object? sender, EventArgs e)
        {
            if (EndEdit is MainRow row )
            {
                if (row.Obj == null) row.CreateNew();
                row.RefrehData();
            }
        }

        private void ИзмененияВнесены(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit) EndEdit = e.Row.Item;
        }

        private void НоваяСтрока(object sender, RoutedEventArgs e)
        {
          
        }

        private void МоиГруппы(object sender, RoutedEventArgs e)
        {
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
    }
}
