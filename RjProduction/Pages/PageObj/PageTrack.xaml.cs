using RjProduction.Model.Catalog;
using RjProduction.WpfFrm;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RjProduction.Pages
{
    public partial class PageTrack : Page, IKeyControl
    {
        private readonly Action CloseAction;
        private readonly TransportPart _transport;
        private Action<TransportPart> ActionOne;
        private MDL.Reference.Catalog<TruckClass> Catalog = new ();

        public PageTrack(TransportPart transport, Action<TransportPart> actionOne, Action closeAction)
        {
            InitializeComponent();
            this.CloseAction = closeAction;
            _transport = transport;
            ActionOne = actionOne;
        }

        private void ОК_Согласие(object sender, RoutedEventArgs e)
        {
            ErrorLabel.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(_transport.AddresTo.Address) || string.IsNullOrEmpty(_transport.AddresFrom.Address)) {
                ErrorLabel.Visibility = Visibility.Visible;
                ErrorLabel.Content = "Необходимо заполнить поля от куда или куда доставка";
                return;
            }

            if (string.IsNullOrEmpty(CarNumber.Text)) {
                ErrorLabel.Visibility = Visibility.Visible;
                ErrorLabel.Content = "Не заполнено поле номер машины";
                return;
            }

            var att = _transport.Truck.GetType();
            RegularExpressionAttribute reg = att.GetProperty(nameof(TruckClass.CarNumber))!.GetCustomAttribute<RegularExpressionAttribute>()!;

            if (!reg.IsValid(CarNumber.Text)) 
            {
                ErrorLabel.Visibility = Visibility.Visible;
                ErrorLabel .Content = reg.ErrorMessage;
                return;
            }
            _transport.Truck.CarNumber = CarNumber.Text;

            reg = att.GetProperty(nameof(TruckClass.TrailerNumber))!.GetCustomAttribute<RegularExpressionAttribute>()!;

            if (!reg.IsValid(TrailerNumber.Text))
            {
                ErrorLabel.Visibility = Visibility.Visible;
                ErrorLabel.Content = reg.ErrorMessage;
                return;
            }
            _transport.Truck.TrailerNumber = TrailerNumber.Text;

            _transport.Truck.CarLabel = CarLabel.Text;
            _transport.Truck.TrailerLabel = TrailerLabel.Text;

            var f = Catalog.ListCatalog.FindIndex(x => x.CarNumber == _transport.Truck.CarNumber);
            if (f == -1)
            {
                // создать новую строку
                Catalog.ListCatalog.Add(_transport.Truck);
                Catalog.SaveData();
                Refreh_Carlist();
            }
            else
            {
                // Проверить на изменения 
                var cl = Catalog.ListCatalog[f];
                if (_transport.Truck.CarLabel != cl.CarLabel ||
                    _transport.Truck.TrailerLabel != cl.TrailerLabel ||
                    _transport.Truck.TrailerNumber != cl.TrailerNumber)
                {
                    Catalog.ListCatalog[f] = _transport.Truck;
                }
                else if (_transport.Truck.CargoCarriers is not null) 
                {
                    if (!_transport.Truck.CargoCarriers.Equals(cl.CargoCarriers)) Catalog.ListCatalog[f] = _transport.Truck;
                }
                
                Catalog.SaveData();
                Refreh_Carlist();
            }

            ActionOne(_transport);
            CloseAction();
        }

        private void Refreh_Carlist() 
        {
            
            List<DeliveredStruct> ls = [];
            foreach (var item in Catalog.ListCatalog)
            {
                if (string.IsNullOrEmpty(item.TrailerNumber)) ls.Add(new DeliveredStruct(item.CarNumber + " " + item.CarLabel, item));
                else ls.Add(new DeliveredStruct(item.CarNumber + " " + item.TrailerNumber + " " + item.CarLabel, item));
            }
            CarList.ItemsSource = ls;
        }

        private void ЗакрытьФорму(object sender, RoutedEventArgs e) => CloseAction?.Invoke();

        public void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1) ОК_Согласие(null!, null!);
            else if (e.Key == Key.Escape) ЗакрытьФорму(null!, null!);
        }

        private void ВыборКомпании(object sender, RoutedEventArgs e)
        {
            Pages.ToolList<Model.Catalog.Company> toolList = new(StartGrid, (obj) =>
            {
                if (obj is Model.Catalog.Company comp)
                {
                    TBox_Company.Text = comp.ShortName;
                    MDL.MyDataBase.CompanyOwn = comp;
                }
            });
        }

        private void ВыборОтКуда(object sender, RoutedEventArgs e)
        {
            WpfFrm.WPF_Address wpf = new(new Model.Catalog.AddresStruct(), (pt) =>
            {
                _transport.AddresFrom = pt;
                Tbox_AddresFrom.Text = pt.ToString();
                Tbox_AddresFrom.ToolTip = "Телефон: " + pt.Phone + " Почта: " + pt.Email;
            });
            wpf.ShowDialog();
        }

        private void ВыборКуда(object sender, RoutedEventArgs e)
        {
            WpfFrm.WPF_Address wpf = new(new Model.Catalog.AddresStruct(), (pt) =>
            {
                _transport.AddresTo = pt;
                Tbox_AddresTo.Text = pt.ToString();
                Tbox_AddresTo.ToolTip = "Телефон: " + pt.Phone + " Почта: " + pt.Email;
            });
            wpf.ShowDialog();
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            ErrorLabel.Visibility = Visibility.Collapsed;
            Refreh_Carlist();
        }

        private void ВыходИзПоля(object sender, RoutedEventArgs e)
        {
            if (!CarNumber.Text.Contains(' ') & CarNumber.Text.Length > 7)
            {
                CarNumber.Text = CarNumber.Text.Insert(6, " ");
            }
        }

        private void ВыходПоля(object sender, RoutedEventArgs e)
        {
            if (!TrailerNumber.Text.Contains(' ') & TrailerNumber.Text.Length > 8)
            {
                TrailerNumber.Text = TrailerNumber.Text.Insert(7, " ");
            }
        }

        private void ВыборОбъекта(object sender, SelectionChangedEventArgs e)
        {
            if (CarList.SelectedValue is DeliveredStruct dl)
            {
                Model.Catalog.TruckClass truck = (TruckClass)dl.Obj!;
                TrailerNumber.Text = truck.TrailerNumber;
                CarNumber.Text = truck.CarNumber;
                CarLabel.Text = truck.CarLabel;
                TrailerLabel.Text = truck.TrailerLabel;
                _transport.Truck.CargoCarriers = truck.CargoCarriers;
                TBox_Company.Text = truck.CargoCarriers?.ShortName;
            }
        }
    }
}
