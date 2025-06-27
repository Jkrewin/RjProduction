using System.Windows.Input;
namespace RjProduction.Pages
{
    /// <summary>
    /// Управление нажатие клавиши для Page
    /// </summary>
    public interface IKeyControl
    {
        /// <summary>
        /// Перехватывает нажатие клавиш для выбранной странници, избегая повторного нажатия на других страницах
        /// </summary>
        public void HandleKeyPress(object sender, KeyEventArgs e);
    }
}
