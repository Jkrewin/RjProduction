using RjProduction.Model.Catalog;
using RjProduction.Model.DocElement;
using RjProduction.WpfFrm;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static RjProduction.Fgis.KindTypes;
using static RjProduction.Fgis.XML.forestUsageReport;

namespace RjProduction.Fgis.WPF
{
    public partial class DicWindows : Window
    {
        private DeliveredStruct[] _dl;
        private readonly Action<DeliveredStruct> _action;
        private TypeElementFgis _TypeElementFgis = TypeElementFgis.none;
        private readonly KindTypes _kind;
        private MDL.Reference.Catalog<Model.Catalog.Company> Catalog_Company;


        public enum TypeElementFgis
        {
            Contract,
            Company,
            Employee,
            none
        }

        public DicWindows(TypesEnum types, Action<DeliveredStruct> action, KindTypes kind)
        {
            InitializeComponent();
            _kind = kind;
            _dl = _kind.GetDate(types);
            _action = action;
        }

        public DicWindows(TypeElementFgis fgis, Action<DeliveredStruct> action, KindTypes kind)
        {
            InitializeComponent();
            _kind = kind;
            _action = action;
            _dl = [];
            Refreh(fgis);
        }

        private void Refreh(TypeElementFgis fgis) {
            if (Catalog_Company.ListCatalog is null) Catalog_Company = new MDL.Reference.Catalog < Model.Catalog.Company > ();

            switch (fgis)
            {
                case TypeElementFgis.Contract:
                    _TypeElementFgis = TypeElementFgis.Contract;
                    _dl = [.. (from tv in MDL.MyDataBase.Contracts where tv.FgisElement is not null select new DeliveredStruct("Договор №" + tv.FgisElement!.number.ToString() + " от " + tv.FgisElement.date.ToString(), Convert.ToInt32(tv.ID), tv.FgisElement.registrationNumber, tv))];
                    break;
                case TypeElementFgis.Company:
                    _TypeElementFgis = TypeElementFgis.Company;
                    _dl = [.. (from tv in Catalog_Company.ListCatalog let w= tv.ToTypeiiOrganizationType  select new DeliveredStruct(w.nameFull + " инн:" + w.inn, Convert.ToInt32(tv.ID), w.nameFull, tv))];
                    break;
                case TypeElementFgis.Employee:
                    _TypeElementFgis = TypeElementFgis.Employee;
                    _dl = [.. (from tv in MDL.MyDataBase.EmployeeDic where string.IsNullOrEmpty(tv.Name_first) ==false & string.IsNullOrEmpty(tv.Name_last) == false select new DeliveredStruct(tv.NameEmployee, 0, tv.ToString(), tv))];
                    break;
                default:
                    _dl = [];
                 break;
            }
            MainList.ItemsSource = _dl;
        }


        private void Создать(object sender, RoutedEventArgs e)
        {
            switch (_TypeElementFgis)
            {
                case TypeElementFgis.Contract:
                    var w = new DialogWindow(new Fgis.XML.forestUsageReport.headerClass.ContractType(), (ob) =>
                    {
                        var c = new Contract();
                        c.FgisElement = (XML.forestUsageReport.headerClass.ContractType?)ob;
                        MDL.MyDataBase.Contracts.Add(c);
                        Refreh(_TypeElementFgis);
                        MainList.Items.Refresh();
                    }, _kind);
                    w.ShowDialog();
                    break;
                case TypeElementFgis.Company:
                    var cc = new DialogWindow(new TypeiiOrganizationType(), (ob) =>
                    {
                        var c = new Company { ToTypeiiOrganizationType = (TypeiiOrganizationType)ob };
                        Catalog_Company.ListCatalog.Add(c);
                        Catalog_Company.SaveData();
                        Refreh(_TypeElementFgis);
                        MainList.Items.Refresh();
                    }, _kind);
                    cc.ShowDialog();
                    break;
                case TypeElementFgis.Employee:
                    var em = new DialogWindow(new PersonNameType(), (ob) => {
                        var c = new Employee();
                        PersonNameType person = (PersonNameType)ob; 
                        c.CreateFullName(person.last, person.first, person.middle);
                        MDL.MyDataBase.EmployeeDic.Add(c);
                        Refreh(_TypeElementFgis);
                        MainList.Items.Refresh();
                    }, _kind);
                    em.ShowDialog();
                    break;
                case TypeElementFgis.none:
                    break;
                default:
                    break;
            }
            MDL.Reference.SaveDB();
        }

        private void Удалить(object sender, RoutedEventArgs e)
        {
            if (MainList.SelectedIndex == -1) return;
            switch (_TypeElementFgis)
            {
                case TypeElementFgis.Contract:                   
                        MDL.MyDataBase.Contracts.RemoveAt(MainList.SelectedIndex);                       
                    break;
                case TypeElementFgis.Company:
                    Catalog_Company.ListCatalog.RemoveAt(MainList.SelectedIndex);
                    Catalog_Company.SaveData();
                    break;
                case TypeElementFgis.Employee:
                    MDL.MyDataBase.EmployeeDic.RemoveAt(MainList.SelectedIndex);
                    break;
                case TypeElementFgis.none:
                    break;
                default:
                    break;
            }
            Refreh(_TypeElementFgis);
            MainList.Items.Refresh();
        }

        private void Изминить(object sender, RoutedEventArgs e)
        {
            if (MainList.SelectedIndex == -1) return;
            DeliveredStruct delivered = (DeliveredStruct)MainList.SelectedItem;
            if (delivered.Obj is null) return;
            object obj = delivered.Obj;

            switch (_TypeElementFgis)
            {
                case TypeElementFgis.Contract:
                    var w = new DialogWindow(new Fgis.XML.forestUsageReport.headerClass.ContractType(), (ob) =>
                    {
                        var c = new Contract();
                        c.FgisElement = (XML.forestUsageReport.headerClass.ContractType?)ob;
                        MDL.MyDataBase.Contracts.Add(c);
                        Refreh(_TypeElementFgis);
                        MainList.Items.Refresh();
                    }, _kind);
                    w.ShowDialog();
                    break;
                case TypeElementFgis.Company:
                    var cc = new DialogWindow(((Company)obj).ToTypeiiOrganizationType, (ob) => {
                        ((Company)obj).ToTypeiiOrganizationType = (TypeiiOrganizationType)ob;
                        
                        Refreh(_TypeElementFgis);
                        MainList.Items.Refresh();
                    }, _kind);
                    cc.ShowDialog();
                    break;
                case TypeElementFgis.Employee:
                    Employee empl = (Employee)obj;
                    var p = new PersonNameType()
                    {
                        last = empl.Name_last,
                        first = empl.Name_first,
                        middle = empl.Name_middle
                    };
                    var em = new DialogWindow(p, (ob) => {
                        PersonNameType  personName = (PersonNameType)ob;
                        empl.CreateFullName (personName.last, personName.first, personName.middle);
                        Refreh(_TypeElementFgis);
                        MainList.Items.Refresh();
                    }, _kind);
                    em.ShowDialog();
                    break;
                case TypeElementFgis.none:
                    break;
                default:
                    break;
            }
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            
            if (_TypeElementFgis == TypeElementFgis.none)
            {
                Button_Create.Visibility = Visibility.Collapsed;
                Button_Del.Visibility = Visibility.Collapsed;
                Button_Edit.Visibility = Visibility.Collapsed;
                MainList.ItemsSource = _dl;
            }

        }

        private void ИзменениеТекста(object sender, TextChangedEventArgs e)
        {
            string str = ((TextBox)sender).Text;
            MainList.ItemsSource = _dl.ToList().FindAll(x => x.Name.Contains(str, StringComparison.OrdinalIgnoreCase));
            MainList.Items.Refresh();
        }

        private void ДвойноеНажатие(object sender, MouseButtonEventArgs e)
        {
            ВыбратьОбъект(null!,null!);
        }

        private void ВыбратьОбъект(object sender, RoutedEventArgs e)
        {
            if (MainList.SelectedIndex != -1) {
                _action((DeliveredStruct)MainList.SelectedItem);
                this.Close();
            }
        }
    }
}
