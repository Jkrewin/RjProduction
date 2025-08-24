
using RjProduction.Model.DocElement;

namespace RjProduction.Model
{
    /// <summary>
    /// Сохраняет в БД только информацию о строках из документа
    /// </summary>
    public class DocRow : Sql.SqlParam, IDoc
    {
        // коды операций
        public const string Пиломатериалы = "A06";
        public const string КруглыйЛес = "A05";
        public const string Сотрудники = "A04";
        public const string Грузоперевозка = "A07";
        public const string Доплата = "B01";


        public string ID_Doc { get; set; }
        public string NameObj { get; set; }
        public decimal Amount { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public string TypeObj { get; set; }
        public string GrupName { get; set; }
        public string Comment { get; set; }
        public double UpRaise { get; set; }
        public float CubatureAll { get; set; }

        public DocRow()
        {
            ID_Doc = string.Empty;
            NameObj = string.Empty;
            TypeObj = string.Empty;
            GrupName = string.Empty;
            Comment = string.Empty;
        }

        public DocRow(string id, string grupname , string comment, string name,double price, double quantity, decimal amount, string type, double upraise, float cubatura) {
            ID_Doc = id;
            GrupName = grupname;
            Comment = comment;
            NameObj = name;
            Price = price;
            Quantity = quantity;
            Amount = amount;
            TypeObj = type;
            UpRaise = upraise;
            CubatureAll = cubatura;
        }

        /// <summary>
        /// Обобщает классы для создание строк в БД по DocRow
        /// </summary>
        public interface IDocRow  {
            /// <summary>
            ///  Создает класс DocRow
            /// </summary>
            /// <returns>DocRow</returns>
            public DocRow ToDocRow(string grupname, string id_document);
            /// <summary>
            /// Отправляет в БД случае если есть в бд такая таблица 
            /// </summary>
            public void Send_DB(string id_doc);
        }
        
    }
}
