
using System.Diagnostics;
using System.Windows;

namespace RjProduction
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {           
            Process thisProc = Process.GetCurrentProcess();          
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                MessageBox.Show("Приложение уже запущено.");
                Application.Current.Shutdown();
                return;
            }
            base.OnStartup(e);
        }
    }

}
