
using System.Windows;

namespace RjProduction.WpfFrm
{
    public partial class WPF_Address : Window
    {
        private Model.Catalog.AddresStruct _Address;
        private Action<Model.Catalog.AddresStruct> Act;
        private MDL.Reference.Catalog<Model.Catalog.AddresStruct> Catalog = new();

        public WPF_Address(Model.Catalog.AddresStruct addres, Action<Model.Catalog.AddresStruct> act)
        {
            InitializeComponent();
            this._Address = addres;
            Act = act;
        }

        private void Добавить(object sender, RoutedEventArgs e)
        {
            var r = MDL.GetStruct<Model.Catalog.AddresStruct>(MainGrind);
            if (Catalog.ExistItem(r.ToString()) == false) Catalog.ListCatalog.Add(r);
            Act(r);
            Catalog.SaveData();
            Close();
        }

        private void Загрузить(object sender, RoutedEventArgs e)
        {
            MDL.SetStruct<Model.Catalog.AddresStruct>(MainGrind, _Address);  
            LBox_Adress.ItemsSource = Catalog.ListCatalog;
        }

        private void ВыборЭлемента(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LBox_Adress.SelectedItem is Model.Catalog.AddresStruct adress)
            {
                _Address = adress;
                MDL.SetStruct<Model.Catalog.AddresStruct>(MainGrind, adress);
            }
        }
    }
}
