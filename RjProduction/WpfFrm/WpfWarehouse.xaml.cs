
using RjProduction.Model;
using System.Windows;
using System.Windows.Media;

namespace RjProduction.WpfFrm
{
    public partial class WpfWarehouse : Window
    {
        const string _TEXT = "<<Создать новый>>";
        readonly List<DeliveredStruct> _warehouses = [];
        readonly Action<WarehouseClass?> _action_exit;
        WarehouseClass? selectedW;


        public WpfWarehouse(Action<WarehouseClass?> action_exit)
        {
            InitializeComponent();
            _action_exit = action_exit;
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            Refreh_DB();

            MainComboBox.ItemsSource = _warehouses;
            MainComboBox.DisplayMemberPath = "Name";

            MainComboBox.SelectedIndex = 0;

            NameWarehouse.Focus();
        }

        private void Refreh_DB() {

            _warehouses.Clear();
            _warehouses.Add(new DeliveredStruct(_TEXT, -1, _TEXT, new Model.WarehouseClass()));           
            for (int i = 0; i < MDL.MyDataBase.Warehouses.Count; i++)
            {
                _warehouses.Add(new DeliveredStruct(MDL.MyDataBase.Warehouses[i].NameWarehouse, i, MDL.MyDataBase.Warehouses[i].DescriptionWarehouse, MDL.MyDataBase.Warehouses[i]));
            }
            MainComboBox.Items.Refresh();
        }

        private void НажитиеЗакрыть(object sender, RoutedEventArgs e)
        {

        }

        private void ВыбраннаСтрока(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MainComboBox.SelectedItem is null) return;
            NameWarehouse.Text = "";
            DescriptionWarehouse.Text = "";
            AddressWarehouse.Text = "";

           selectedW = ((WarehouseClass)((DeliveredStruct)MainComboBox.SelectedItem).Obj!);

            string s = ((DeliveredStruct)MainComboBox.SelectedItem).Name;
            if (s == _TEXT)
            {
                MainButton.Content = "Добавить и закрыть форму";
                ButtonSelect.Visibility = Visibility.Hidden;
            }
            else
            {
                MDL.ImportToWpf(this, ((WarehouseClass)((DeliveredStruct)MainComboBox.SelectedItem).Obj!));
                MainButton.Content = "Внести изменения";
                ButtonSelect.Visibility = Visibility.Visible;
            }
        }

        private void ОбработкаОбъекта(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NameWarehouse.Text))
            {
                NameWarehouse.BorderBrush = Brushes.DarkRed;
                return;
            }

            if (MainComboBox.SelectedItem is null) return;
           
            DeliveredStruct dv= (DeliveredStruct)MainComboBox.SelectedItem;

            if (dv.Name == _TEXT)
            {
                if (MDL.MyDataBase.Warehouses.Any(x => x.NameWarehouse.Equals(NameWarehouse.Text, StringComparison.CurrentCultureIgnoreCase)))
                {
                    MessageBox.Show("Склад с таким названием уже существует!");
                    return;
                }
                MDL.MyDataBase.Warehouses.Add(MDL.ExportFromWpf<WarehouseClass>(this,new WarehouseClass()));
                MDL.SaveXml<MDL.Reference>(MDL.MyDataBase, MDL.SFile_DB);
                this.Close();
                return;
            }

            // Если нужно изменить строку    
            WarehouseClass warehouse = (WarehouseClass)MDL.ExportFromWpf(this, (Sql.SqlParam)dv.Obj!);
            MDL.MyDataBase.Warehouses[dv.ID] = warehouse;
            if (MessageBox.Show("Обновить данные так же в общей БД ? ", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                Sql.SqlRequest.SetData(warehouse);
            }

            MDL.SaveXml<MDL.Reference>(MDL.MyDataBase, MDL.SFile_DB);
            Refreh_DB();
        }

        private void ФормаЗакрыта(object sender, EventArgs e)
        {           
            _action_exit(selectedW);
        }

        private void ВыборБД(object sender, RoutedEventArgs e)
        {
            Refreh_DB();
        }

        private void Синхронизация(object sender, RoutedEventArgs e)
        {           
            var ls = Sql.SqlRequest.ReadСollection<WarehouseClass>(nameof(WarehouseClass), nameof(WarehouseClass.ActiveObjIsDelete) + "='0'");
            if (ls is null) return;
            foreach (var item in ls)
            {
                var l = MDL.MyDataBase.Warehouses.FindIndex(x => x.Equals(item));
                if (l == -1)
                {
                    //добавить если нет в локальной базе 
                    MDL.MyDataBase.Warehouses.Add(item);
                }
                else
                {
                    // если есть то обновить 
                   if (  MDL.MyDataBase.Warehouses[l].SyncData != item.SyncData) MDL.MyDataBase.Warehouses[l]= item;
                }
            }

            Refreh_DB();
            MainButton_Синхрон.Visibility = Visibility.Collapsed;
        }

        private void ВыборОбъекта(object sender, RoutedEventArgs e) => Close();

        private void Вход_поле(object sender, RoutedEventArgs e)
        {
            WpfFrm.WPF_Address wpf = new(new Model.Catalog.AddresStruct(), (pt) =>
            {
                AddressWarehouse.Tag= pt;
                AddressWarehouse.Text = pt.ToString();
                AddressWarehouse.ToolTip = "Телефон: " + pt.Phone + " Почта: " + pt.Email;
            });
            wpf.ShowDialog();
        }
    }
}
