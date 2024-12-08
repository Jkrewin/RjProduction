using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static RjProduction.Model.Document;

namespace RjProduction.Pages
{
    public partial class PageStaff : Page
    {
        private readonly Action<IDoc> ActionOne;
        private readonly Action CloseAction;
        private readonly Employee _Employe;

        public PageStaff(Employee employee, Action<IDoc> actionOne, Action closeAction)
        {
            InitializeComponent();
            ActionOne = actionOne;
            CloseAction = closeAction;
            _Employe = employee;
        }


        private void ОК_СогласиеСотрудник(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ComBoxEmpl.Text)) { ComBoxEmpl.Background = Brushes.OrangeRed; return; }
            if (CBox_IsWorker.IsChecked == false) if (string.IsNullOrEmpty(TextBoxMoneySpend.Text)) { TextBoxMoneySpend.Background = Brushes.OrangeRed; return; }

            if (!MDL.MyDataBase.EmployeeDic.Any(x => x.Equals(ComBoxEmpl.Text, StringComparison.CurrentCultureIgnoreCase)))
            {
                MDL.MyDataBase.EmployeeDic.Add(ComBoxEmpl.Text);
            }

            ActionOne?.Invoke(MDL.GetStruct<Employee>(MainGrid));
            ЗакрытьФорму(null!, null!);
        }

        private void ЗакрытьФорму(object sender, RoutedEventArgs e) => CloseAction?.Invoke();
        private void Очистить_поле(object sender, RoutedEventArgs e) => TextBoxMoneySpend.Text = "";

        private void ВыбранаПроизводстве(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true) TextBoxMoneySpend.Visibility = Visibility.Hidden;
            else TextBoxMoneySpend.Visibility = Visibility.Visible;
        }

        private void ПроеркаНаВвод(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(TextBoxMoneySpend.Text, out _)) TextBoxMoneySpend.Text = string.Empty;
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            MDL.SetStruct(MainGrid, _Employe);
            ВыбранаПроизводстве(CBox_IsWorker, null!);
            ComBoxEmpl.ItemsSource = MDL.MyDataBase.EmployeeDic;
            PreviewKeyUp += (object sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.F1) ОК_СогласиеСотрудник(null!, null!);
                else if (e.Key == Key.Escape) CloseAction?.Invoke();
            };
        }
    }
}
