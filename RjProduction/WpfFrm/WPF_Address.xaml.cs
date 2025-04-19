using RjProduction.Model;
using System.Windows;

namespace RjProduction.WpfFrm
{
    public partial class WPF_Address : Window
    {
        private Model.Catalog.AddresStruct _Address;
        private Action<Model.Catalog.AddresStruct> Act;


        public WPF_Address(Model.Catalog.AddresStruct addres, Action<Model.Catalog.AddresStruct> act)
        {
            InitializeComponent();
            this._Address = addres;
            Act = act;
        }

        private void Добавить(object sender, RoutedEventArgs e)
        {
            Act(MDL.GetStruct<Model.Catalog.AddresStruct>(MainGrind));
            Close();
        }

        private void Загрузить(object sender, RoutedEventArgs e)
        {
            MDL.SetStruct<Model.Catalog.AddresStruct>(MainGrind,_Address);
            List<DeliveredStruct> ls = [];

            foreach (var item in MDL.MyDataBase.Warehouses)
            {
                ls.Add(new DeliveredStruct(item.NameWarehouse + ": "+ item.AddressWarehouse, 0,item.NameWarehouse,item));
            }

            LBox_Adress.ItemsSource = ls;
            LBox_Adress.DisplayMemberPath = "Name";
        }

        private void ВыборЭлемента(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
if (LBox_Adress.SelectedItem is DeliveredStruct delivered) {
                if (delivered.Obj is WarehouseClass warehouse) {
                    _Address = warehouse.InfoWarehouse;
                    MDL.SetStruct<Model.Catalog.AddresStruct>(MainGrind, _Address);
                }            
            }
        }
    }
}
