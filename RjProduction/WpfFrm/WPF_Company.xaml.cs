using RjProduction.Fgis;
using RjProduction.Model.Catalog;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static RjProduction.Model.Catalog.Company;


namespace RjProduction.WpfFrm
{
    public partial class WPF_Company : Window
    {
        private readonly Action<Company> Act;
        private readonly Model.Catalog.Company Company;
        private (TypeCompanyEnum, string)[] ListType = [
            (TypeCompanyEnum.PrivatePerson  , "Физическое лицо РФ (за исключением ИП)"),
            (TypeCompanyEnum.Businessman  , "Индивидуальный предприниматель РФ"),
            (TypeCompanyEnum.Organization  , "Юридическое лицо РФ"),
            (TypeCompanyEnum.ForeignOrganization  , "Юридическое лицо иностранного государства"),
            (TypeCompanyEnum.ForeignBusinessman  , "Индивидуальный предприниматель иностранного государства")
        ];

        public WPF_Company(Model.Catalog.Company company, Action<Company> act)
        {
            InitializeComponent();
            Company = company;
            Act = act;
        }

        private void АдресВыбрать(object sender, RoutedEventArgs e)
        {
            WpfFrm.WPF_Address wpf = new(new Model.Catalog.AddresStruct(), (pt) =>
            {
                Company.Address = pt;
                TBox_Address.Text = pt.ToString();
                TBox_Address.ToolTip = "Телефон: " + pt.Phone + " Почта: " + pt.Email;
            });
            wpf.ShowDialog();
        }

        private void ЗакрытьОкно(object sender, RoutedEventArgs e) => Close();

        private void ДействиеВыполнить(object sender, RoutedEventArgs e)
        {
            bool error = false;
            foreach (var item in Company.GetType().GetProperties())
            {
                Mod? mod = Mod.Extract_Mod(item);
                if (mod is not null)
                {
                    if (this.FindName(item.Name) is TextBox text)
                    {
                        if (mod.MType == Mod.MTypeEnum.Optional & string.IsNullOrEmpty(text.Text)) 
                        {
                            text.Background = Brushes.White;
                        }
                        else if (Mod.CheckRule(text.Text, mod))
                        {
                            text.Background = Brushes.White;
                        }
                        else
                        {
                            error = true;
                            text.Background = Brushes.MistyRose;
                        }
                    }
                }
            }
            if (error) return;

            Company.TypeCompany = ListType[CBox_Type.SelectedIndex].Item1;
            Act(MDL.ExportFromWpf<Model.Catalog.Company>(this, Company));
            ЗакрытьОкно(null!, null!);
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            MDL.ImportToWpf(this, Company);
            foreach (var item in ListType) CBox_Type.Items.Add(item.Item2);          

            if (string.IsNullOrEmpty(Company.Address.Address))
            {
                TBox_Address.Text = Company.Address.ToString();
                TBox_Address.ToolTip = "Телефон: " + Company.Address.Phone + " Почта: " + Company.Address.Email;
            }

            if (string.IsNullOrEmpty(Company.FullName) == false) MainButton_OK.Content = "Внести изменения";           

            for (int i = 0; i < ListType.Length; i++)
            {
                if (ListType[i].Item1 == Company.TypeCompany) {
                    CBox_Type.SelectedIndex = i;
                    break;
                }
            }


        }
    }
}
