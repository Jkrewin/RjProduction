using System.Windows.Input;
namespace RjProduction.Pages
{
    /// <summary>
    /// Управление нажатие клавиши для Page
    /// </summary>
    public interface IKeyControl
    {
        public void HandleKeyPress(object sender, KeyEventArgs e);
    }
}
