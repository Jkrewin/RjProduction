using RjProduction.Model.Catalog;
using RjProduction.Model.Classifier;
using System.Windows.Media.Effects;

namespace RjProduction.XML.BlankDoc
{
    /// <summary>
    /// Накладная
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Грузоотправитель
        /// </summary>
        public required Company Shipper { get; set; }
        /// <summary>
        /// Поставщик
        /// </summary>
        public required Company Supplier { get; set; }
        /// <summary>
        /// Плательщик
        /// </summary>
        public required Company Payer { get; set; }
        /// <summary>
        /// Основание
        /// </summary>
        public Contract? Footing { get; set; }
        /// <summary>
        /// Номер накладной
        /// </summary>
        public ushort Number { get; set; }
        /// <summary>
        /// Дата накладной
        /// </summary>
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        /// <summary>
        /// Табличная часть 
        /// </summary>
        public List<RowTabel> Tabel = [];

        /// <summary>
        /// Сохранить в файл 
        /// </summary>
        /// <param name="sFile">Путь к куда сохранить название файла тоже</param>
        public void SaveDoc(string sFile) { 
        
        }
        /// <summary>
        /// Загрузить документ 
        /// </summary>
        /// <param name="sFile">Путь и занвание файла где сохраинть </param>
        public void LoadDoc(string sFile)
        { 
        
        }
        /// <summary>
        /// Генерирует отчет
        /// </summary>
        public void GeneratorReport(TypeInvoiceEnum type)
        {

        }



        /// <summary>
        /// Таблица для хранения строк
        /// </summary>
        public class RowTabel : Сommodity
        {
            public string UnitName { get => Packaging.UnitMeas.Name; }
            public ushort UnitCode { get => Packaging.UnitMeas.Code; }
            public ushort Num { get; set; }
        }

        /// <summary>
        /// Типы накладных 
        /// </summary>
        public enum TypeInvoiceEnum
        { 
            Торг12,
            Обычная
        }

      
    }
}
