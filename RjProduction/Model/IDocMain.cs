
using System.Windows;

namespace RjProduction.Model
{
    /// <summary>
    /// Структура документа
    /// </summary>
   public interface IDocMain
    {
        public string Doc_Code { get; }
        public StatusEnum Status { get; set; }
        /// <summary>
        /// Необходимый индификатор для общей бд бля быстрого поиска этого документа
        /// </summary>
        public string ID_Doc { get; }    
        public static string? VerDoc { get ; }
        /// <summary>
        /// Дата создания документа
        /// </summary>
        public DateOnly DataCreate { get; set; }
        /// <summary>
        /// Номер документа, также сохраняет последний номер в MDL
        /// </summary>
        public uint Number { get; set; }
        /// <summary>
        /// Заголовок документа
        /// </summary>
        public string DocTitle { get; set; }
        /// <summary>
        /// Главная  таблица
        /// </summary>
        public List<Model.DocElement.GrupObj> MainTabel { get; set; }

        /// <summary>
        /// Провести документ
        /// </summary>
        public void CarryOut();
        /// <summary>
        /// Общие сообщения об ошибке
        /// </summary>
        public static void ErrorMessage(Error_Txt m)
        {
            switch (m)
            {
                case Error_Txt.Ошибка_в_документе:
                    MessageBox.Show("Этот документ был ранее проведен или произошла ошибка. С такой датой и номером. Уже зафиксированы в БД изменения, если вам нужно внести изменения, откройте этот документ, внесите изменения (если нужны) и измените дату и номер документа ");
                    break;
                case Error_Txt.Нет_подключенияБД:
                    MessageBox.Show("Нет активного подключения к БД, создайте новое подключение к БД.");
                    break;
                default:
                    MessageBox.Show("Ошибка в документе, проверьте правильность заполнения документа.");
                    break;
            }
        }

        public enum Error_Txt
        {
           Ошибка_в_документе,
           Нет_подключенияБД,
           none
        }
    }

    /// <summary>
    /// Helper в конвертации. Конвертурует только физически сущемтвующее материалы на складе для учета в Products
    /// </summary>
    public interface IConvertDoc {
        public Products ToProducts();

    }
}
