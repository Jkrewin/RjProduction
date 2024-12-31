
using System.Windows;

namespace RjProduction.WpfFrm
{
    
    public partial class ErrorLog : Window
    {
        public ErrorLog(string title_error, string mess_error)
        {
            InitializeComponent();
            TitleError.Content = title_error;
            ErrorText.Text = mess_error;
        }
    }
}
