
using System.Windows;

namespace RjProduction.WpfFrm
{


    public partial class ErrorLog : Window
    {
        bool InfoProfile = false;

        public ErrorLog(string title_error, string mess_error)
        {
            InitializeComponent();
            TitleError.Content = title_error;
            ErrorText.Text = mess_error;
        }

        private void Закрыть(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ИнфПодключение(object sender, RoutedEventArgs e)
        {
            if (InfoProfile == false)
            {
                string s;
                if (MDL.SqlProfile is null)
                {
                    s = "   -- подключение отсутствует --";
                }
                else
                {
                    s = "   SqlProfile(" + MDL.SqlProfile.ToString() + "):" + MDL.SqlProfile.SqlLogString;
                }
                ErrorText.Text += s;
                InfoProfile = !InfoProfile;
            }
        }

        private void Отправить(object sender, RoutedEventArgs e)
        {

            Закрыть(null!,null!);
        }
    }
}
