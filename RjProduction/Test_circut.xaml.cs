using RjProduction.Model;
using System;
using System.Collections.Generic;
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
using HtmlAgilityPack;
using System.IO;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Controls.Primitives;

namespace RjProduction
{
    public partial class Test_circut : Window
    {


     /*   #region "Ридер строк таблици по UI"
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
     */



        public Test_circut()
        {
            InitializeComponent();
           
        }

        private void ТестА(object sender, RoutedEventArgs e)
        {
           
           /* var y = Sql.SqlRequest.ReadData<DocRow>(1);
            y.Price = 777;
            Sql.SqlRequest.SetData(y);*/

           //t.Rollback();
        }

        private void ТестБ(object sender, RoutedEventArgs e)
        {
            string sfile = "C:\\Users\\Макс\\Desktop\\Новая папка\\test.xml";
            Fgis.XML.forestUsageReport forest = new Fgis.XML.forestUsageReport();
            forest.SaveXml(sfile);

           

        }

        private void ТестC(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
