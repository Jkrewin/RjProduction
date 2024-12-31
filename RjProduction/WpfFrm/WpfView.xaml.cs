
using System.Windows;
using System.Windows.Controls;

namespace RjProduction.WpfFrm
{
    
    public partial class WpfView : Window
    {
        private Page? _page;
        private Action? ClosePage;  //Ссылка на метод закрытия это формы


        public WpfView(Page page, Action closePage)
        {
            InitializeComponent();
            _page = page;
            ClosePage = closePage;
        }

        public WpfView()
        {
            InitializeComponent();
        }

        public void LoadPage(Page page, Action closePage) {
            _page = page;
            ClosePage = closePage;
            FrameDisplay.Navigate(_page);
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            FrameDisplay.Navigate (_page);           
        }

        private void ФормаЗакрыта(object sender, EventArgs e)
        {
            ClosePage?.Invoke();
        }
    }
}
