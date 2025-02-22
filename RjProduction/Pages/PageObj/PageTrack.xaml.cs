using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RjProduction.Pages
{
    public partial class PageTrack : Page, IKeyControl
    {
        private readonly Action CloseAction;

        public PageTrack(Action closeAction)
        {
            InitializeComponent();
            this.CloseAction = closeAction;
        }

        private void ОК_Согласие(object sender, RoutedEventArgs e)
        {

        }

        private void ЗакрытьФорму(object sender, RoutedEventArgs e)
        {

        }

        public void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1) ОК_Согласие(null!, null!);
            else if (e.Key == Key.Escape) CloseAction?.Invoke();
        }
    }
}
