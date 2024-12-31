using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RjProduction.Model;

namespace RjProduction.Pages
{
    public partial class PageSurcharges : Page
    {
        private readonly Action<IDoc> ActionOne;
        private readonly Action CloseAction;
        private Surcharges _surcharges;

        public PageSurcharges(Surcharges surcharges, Action<IDoc> actionOne, Action closeAction)
        {
            InitializeComponent();
            CloseAction = closeAction;
            ActionOne = actionOne;
            _surcharges = surcharges;
        }

        private void ОК_Согласие(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Number.Text))
            {
                Number.BorderBrush = Brushes.Red;
            }
            else
            {
                _surcharges = MDL.GetStruct<Surcharges>(MainGrid);

                ActionOne?.Invoke(_surcharges);
                ЗакрытьФорму(null!, null!);
            }

        }

        private void ЗакрытьФорму(object sender, RoutedEventArgs e) => CloseAction?.Invoke();

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            ComBoxEmpl.ItemsSource = MDL.MyDataBase.EmployeeDic;
            Number.Text = _surcharges.UpRaise.ToString();
            if (_surcharges.TypeSurcharges == Surcharges.TypeSurchargesEnum.ДоплатаЦене)
            {
                ДоплПоЦене(null!, null!);
                RB1.IsChecked = true;
            }
            else
            {
                RB2.IsChecked = true;
                ДоплптаПоЗарплате(null!, null!);
            }
            InfoBox.Text = _surcharges.Info;
            if (string.IsNullOrEmpty(_surcharges.EmployeeName) == false)
            {
                var r = MDL.MyDataBase.EmployeeDic.FindIndex(x => x.Equals(_surcharges.EmployeeName));
                // если поиндексу ничего не найдено следует добавить ранее удаленный вариат
                if (r == -1)
                {
                    ComBoxEmpl.Text = "asdasdasd";
                }
                else ComBoxEmpl.SelectedIndex = r;
            }
                        
            PreviewKeyUp += (object sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.F1) ОК_Согласие(null!, null!);
                else if (e.Key == Key.Escape) CloseAction?.Invoke();
            };
            if (Number.Text == "0") Number.Focus();
        }

        private void ДоплПоЦене(object sender, RoutedEventArgs e)
        {
            TypeSurcharges.Text = "ДоплатаЦене";
            _surcharges.EmployeeName = string.Empty;
            ComBoxEmpl.IsEnabled = false;
        }

        private void ДоплптаПоЗарплате(object sender, RoutedEventArgs e)
        {
            TypeSurcharges.Text = "ДоплатаСумме";
            ComBoxEmpl.IsEnabled = true;
        }

        private void ПроверкаЦифр(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(Number.Text, out double d) == false) Number.Text = _surcharges.UpRaise.ToString();
            else _surcharges.UpRaise = d;
        }

        private void ВходВПоле(object sender, RoutedEventArgs e) => Number.Text = "";
    }
}
