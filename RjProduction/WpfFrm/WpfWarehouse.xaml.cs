
using RjProduction.Model;
using System.Windows;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace RjProduction.WpfFrm
{
    public partial class WpfWarehouse : Window
    {
        const string _TEXT = "<<Создать новый>>";
        readonly List<DeliveredStruct> _warehouses = [];
        readonly Action _action_exit;


        public WpfWarehouse(Action action_exit)
        {
            InitializeComponent();
            _action_exit = action_exit;
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            _warehouses.Add(new DeliveredStruct ( _TEXT,-1,_TEXT,new Model.WarehouseClass()));
            foreach (var item in MDL.MyDataBase.Warehouses) _warehouses.Add(new DeliveredStruct(item.NameWarehouse , (int)item.ID, item.DescriptionWarehouse, item));
                       
            MainComboBox.ItemsSource = _warehouses;
            MainComboBox.DisplayMemberPath = "Name";

            MainComboBox.SelectedIndex = 0;

            NameWarehouse.Focus();
        }
             
        private void НажитиеЗакрыть(object sender, RoutedEventArgs e)
        {

        }

        private void ВыбраннаСтрока(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            NameWarehouse.Text = "";
            DescriptionWarehouse.Text = "";
            AddressWarehouse.Text = "";

            string s = ((DeliveredStruct)MainComboBox.SelectedItem).Name;
            if (s == _TEXT)
            {
                MainButton.Content = "Добавить и закрыть форму";
            }
            else
            {
                MDL.ImportToWpf(this, ((WarehouseClass)((DeliveredStruct)MainComboBox.SelectedItem).Obj!));
                MainButton.Content = "Внести изменения";
            }
        }

        private void ОбработкаОбъекта(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NameWarehouse.Text))
            {
                NameWarehouse.BorderBrush = Brushes.DarkRed;
                return;
            }

            string s = ((DeliveredStruct)MainComboBox.SelectedItem).Name;
            if (s == _TEXT)
            {
                MDL.MyDataBase.Warehouses.Add(MDL.ExportFromWpf<Model.WarehouseClass>(this));
                MDL.SaveXml<MDL.Reference>(MDL.MyDataBase, MDL.SFile_DB);
                this.Close();
                return;
            }

            var r = MDL.MyDataBase.Warehouses.FindAll(x => x.NameWarehouse.Equals(s, StringComparison.CurrentCultureIgnoreCase));
            if (r.Count >= 2)
            {
                MessageBox.Show("Склад с таким названием уже существует!");
                return;
            }
            var index = MDL.MyDataBase.Warehouses.FindIndex(x => x == r[0]);
            MDL.MyDataBase.Warehouses[index] = MDL.ExportFromWpf<Model.WarehouseClass>(this);

            MDL.SaveXml<MDL.Reference>(MDL.MyDataBase, MDL.SFile_DB);
        }

        private void ФормаЗакрыта(object sender, EventArgs e)
        {
            _action_exit.Invoke();
        }
    }
}
