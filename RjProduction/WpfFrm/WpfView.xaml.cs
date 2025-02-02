using System.Windows;
using System.Windows.Controls;

namespace RjProduction.WpfFrm
{

    public partial class WpfView : Window
    {
        private Page? _page;
        private Action? ClosePage;  //Ссылка на метод закрытия это формы

       
        

        public WpfView(Page page,string title, Action closePage)
        {
            InitializeComponent();
            _page = page;
            ClosePage = closePage;
            Title = title;
        }

        public WpfView()
        {
            InitializeComponent();
        }

        public void LoadPage(Page page, Action closePage)
        {
            _page = page;
            ClosePage = closePage;
            FrameDisplay.Navigate(_page);
        }

        private void Загруженно(object sender, RoutedEventArgs e)
        {
            FrameDisplay.Navigate(_page);
            MDL.AllWpfViewWin.Add(this);
            MDL.Refreh_AllWpfView();
        }

        private void ФормаЗакрыта(object sender, EventArgs e)
        {
            MDL.AllWpfViewWin.Remove(this);
            MDL.Refreh_AllWpfView();
            ClosePage?.Invoke();
        }

        private void ФокусФормыПотерян(object sender, RoutedEventArgs e)
        {
            MDL.Refreh_AllWpfView();
        }
    }
}