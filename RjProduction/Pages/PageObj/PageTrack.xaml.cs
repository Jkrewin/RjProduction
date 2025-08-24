using RjProduction.Model.Catalog;
using RjProduction.Model.DocElement;
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
        private readonly Transportation _transport;
        private Action<Transportation> ActionOne;
        private MDL.Reference.Catalog<TransportPart> Catalog = new ();
        private AddresStruct? Adress_From;

        public PageTrack(Transportation transport, Action<Transportation> actionOne, Action closeAction)
        {
            InitializeComponent();
            this.CloseAction = closeAction;
            _transport = transport;
            ActionOne = actionOne;
        }

        public PageTrack(Transportation transport, Action<Transportation> actionOne, Action closeAction, AddresStruct from_addres )
        {
            InitializeComponent();
            this.CloseAction = closeAction;
            _transport = transport;
            ActionOne = actionOne;
            Adress_From = from_addres;
        }

        private void ОК_Согласие(object sender, RoutedEventArgs e)
        {
            ErrorLabel.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(_transport.EndPlace.Address) || string.IsNullOrEmpty(_transport.StartPlace.Address)) {
                ErrorLabel.Visibility = Visibility.Visible;
                ErrorLabel.Content = "Необходимо заполнить поля от куда или куда доставка";
                return;
            }

            if (string.IsNullOrEmpty(CarNumber.Text)) {
                ErrorLabel.Visibility = Visibility.Visible;
                ErrorLabel.Content = "Не заполнено поле номер машины";
                return;
            }

            var att = _transport.Transport.GetType();
            RegularExpressionAttribute reg = att.GetProperty(nameof(TruckClass.CarNumber))!.GetCustomAttribute<RegularExpressionAttribute>()!;

            if (!reg.IsValid(CarNumber.Text)) 
            {
                ErrorLabel.Visibility = Visibility.Visible;
                ErrorLabel .Content = reg.ErrorMessage;
                return;
            }
            _transport.Transport.CarNumber = CarNumber.Text;

            reg = att.GetProperty(nameof(TruckClass.TrailerNumber))!.GetCustomAttribute<RegularExpressionAttribute>()!;

            if (!reg.IsValid(TrailerNumber.Text))
            {
                ErrorLabel.Visibility = Visibility.Visible;
                ErrorLabel.Content = reg.ErrorMessage;
                return;
            }
            _transport.Transport.TrailerNumber = TrailerNumber.Text;

            _transport.Transport.CarLabel = CarLabel.Text;
            _transport.Transport.TrailerLabel = TrailerLabel.Text;

            var f = Catalog.ListCatalog.FindIndex(x => x.CarNumber == _transport.Transport.CarNumber);
            if (f == -1)
            {
                // создать новую строку
                Catalog.ListCatalog.Add(_transport.Transport  );
                Catalog.SaveData();
                Refreh_Carlist();
            }
            else
            {
                // Проверить на изменения 
                var cl = Catalog.ListCatalog[f];
                if (_transport.Transport.CarLabel != cl.CarLabel ||
                    _transport.Transport.TrailerLabel != cl.TrailerLabel ||
                    _transport.Transport.TrailerNumber != cl.TrailerNumber)
                {
                    Catalog.ListCatalog[f] = _transport.Transport;
                }
                else if (_transport.Transport.CargoCarriers is not null) 
                {
                    if (!_transport.Transport.CargoCarriers.Equals(cl.CargoCarriers)) Catalog.ListCatalog[f] = _transport.Transport ;
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
                    _transport.Transport.CargoCarriers = comp;
                }
            });
        }

        private void ВыборОтКуда(object sender, RoutedEventArgs e)
        {
            WpfFrm.WPF_Address wpf = new(new Model.Catalog.AddresStruct(), (pt) =>
            {
                _transport.StartPlace = pt;
                Tbox_AddresFrom.Text = pt.ToString();
                Tbox_AddresFrom.ToolTip = "Телефон: " + pt.Phone + " Почта: " + pt.Email;
            });
            wpf.ShowDialog();
        }

        private void ВыборКуда(object sender, RoutedEventArgs e)
        {
            WpfFrm.WPF_Address wpf = new(new Model.Catalog.AddresStruct(), (pt) =>
            {
                _transport.EndPlace = pt;
                Tbox_AddresTo.Text = pt.ToString();
                Tbox_AddresTo.ToolTip = "Телефон: " + pt.Phone + " Почта: " + pt.Email;
            });
            wpf.ShowDialog();
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            ErrorLabel.Visibility = Visibility.Collapsed;
            Refreh_Carlist();
            if (Adress_From.HasValue) // по умолчанию куда 
            {
                _transport.StartPlace = Adress_From.Value;
                Tbox_AddresTo.Text = _transport.EndPlace.ToString();
                Tbox_AddresTo.ToolTip = "Телефон: " + _transport.EndPlace.Phone + " Почта: " + _transport.EndPlace.Email;
                ButtonTo.Visibility = Visibility.Hidden;
            }
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
                _transport.Transport.CargoCarriers = truck.CargoCarriers;
                TBox_Company.Text = truck.CargoCarriers?.ShortName;
            }
        }
    }
}
