
using RjProduction.Model.Catalog;
using RjProduction.Model.DocElement;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RjProduction.Pages.PageObj
{
    
    public partial class PageFixCub : Page, IKeyControl
    {
        private readonly Action CloseAction;
        private readonly FixCub _fix;
        private readonly Action<FixCub> ActionOne;

        public PageFixCub(FixCub fix, Action<FixCub> actionOne, Action closeAction)
        {
            InitializeComponent();
            CloseAction = closeAction;
            _fix = fix;
            ActionOne = actionOne;
        }

        private void ОК_Согласие(object sender, RoutedEventArgs e)
        {
            Fact.Background = Brushes.White;
            Price.Background = Brushes.White;
            Plan.Background = Brushes.White;

            if (double.TryParse(Plan.Text, out double plan)==false) { Plan.Background = Brushes.Pink; return; }

            if (double.TryParse(Fact.Text, out double fact))
            {
                if (double.TryParse(Price.Text, out double price))
                {
                    _fix.Fact = fact;
                    _fix.Price = price;
                    _fix.Plan = plan;
                    ActionOne.Invoke(_fix);
                    CloseAction?.Invoke();
                }
                else
                {
                    Price.Background = Brushes.Pink;
                }
            }
            else
            {
                Fact.Background = Brushes.Pink;
            }
        }

        public void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1) ОК_Согласие(null!, null!);
            else if (e.Key == Key.Escape) CloseAction?.Invoke();
        }

        private void ЗакрытьФорму(object sender, RoutedEventArgs e) => CloseAction?.Invoke();

        private void ИзменениеПоле(object sender, TextChangedEventArgs e)
        {
            if (double .TryParse(Fact.Text,out double fact)) {
                if (double.TryParse(Plan.Text, out double plan))
                {
                    Label_l1.Content = plan - fact;
                }
                else {
                    Label_l1.Content = 0;
                }
                if (double.TryParse(Price.Text, out double price))
                {
                    Label_itog.Content = price * fact;
                }
            }            
        }

        private void ПрименитьРасценку(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(Price_Down.Text, out double down)) MDL.MyDataBase.SalaryRates.Price_Down = down;
            if (double.TryParse(Price_Up.Text, out double up)) MDL.MyDataBase.SalaryRates.Price_Up = up;
            if (byte.TryParse(Proc_Price_Up.Text, out byte up1)) MDL.MyDataBase.SalaryRates.Proc_Price_Up = up1;
            if (byte.TryParse(Proc_Price_Down.Text, out byte down1)) MDL.MyDataBase.SalaryRates.Proc_Price_Down = down1;

            if (double.TryParse(Fact.Text, out double fact))
            {
                if (double.TryParse(Plan.Text, out double plan))
                {
                    if ((fact * (MDL.MyDataBase.SalaryRates.Proc_Price_Up / 100)) + fact >= plan)
                    {
                        // есть привышение плана
                        Price.Text = MDL.MyDataBase.SalaryRates.Price_Up.ToString();
                    }
                    else if ((fact * (MDL.MyDataBase.SalaryRates.Proc_Price_Up / 100)) + fact <= plan)
                    {
                        // не выполнение плана
                        Price.Text = MDL.MyDataBase.SalaryRates.Price_Down.ToString();
                    }
                    else {
                        // середина
                        Price.Text = MDL.MyDataBase.SalaryRates.Price_Standart.ToString();
                    }
                }
            }
        }

        private void Загрузка(object sender, RoutedEventArgs e)
        {
            MDL.ImportToWpf(this, MDL.MyDataBase.SalaryRates);
            MDL.ImportToWpf(this, _fix);
        }
    }
}
