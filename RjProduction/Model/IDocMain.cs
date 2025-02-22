
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
        public List<GrupObj> MainTabel { get; set; }

        /// <summary>
        /// Провести документ
        /// </summary>
        public void CarryOut();       
    }

    /// <summary>
    /// Helper в конвертации
    /// </summary>
    public interface IConvertDoc {
        public Products ToProducts();

    }
}
