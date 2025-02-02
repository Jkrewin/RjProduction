

namespace RjProduction.Model
{
    /// <summary>
    /// Сохраняет в БД только информацию о строках из документа
    /// </summary>
    public class DocRow: Sql.SqlParam, IDoc
    {
        // коды операций
        public const string Пиломатериалы = "A06";
        public const string КруглыйЛес = "A05";
        public const string Сотрудники = "A04";
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
        public double CubatureAll { get; set; }

        public DocRow() {
            ID_Doc = string.Empty;
            NameObj = string.Empty;
            TypeObj = string.Empty;
            GrupName = string.Empty;
            Comment = string.Empty;
        }

        public DocRow(IDoc doc, string grupname, string id_document) {
            ID_Doc = id_document;
            GrupName = grupname;
            Comment =string.Empty;
            if (doc is MaterialObj material)
            {
                NameObj = material.NameMaterial;
                Price = material.Price;
                Quantity = material.Quantity;
                Amount = material.Amount;
                TypeObj = Пиломатериалы;
                UpRaise = material.UpRaise;
                CubatureAll = material.CubatureAll;
            }
            else if (doc is Tabel_Timbers timber)
            {
                NameObj = timber.ToProducts().NameItem;
                Price = -1;
                Quantity = timber.Timbers.Sum(x => x.Количество);
                Amount = timber.Amount;
                TypeObj = КруглыйЛес;
                UpRaise = timber.UpRaise;
                CubatureAll = timber.CubatureAll;
            }
            else if (doc is Employee empl)
            {
                NameObj = empl.NameEmployee;
                Price = empl.Payment;
                Quantity = 0;
                Amount = empl.Amount;
                TypeObj = Сотрудники;
                UpRaise = empl.UpRaise;
                CubatureAll = 0;
                Comment = "Оплата: " + (empl.Worker ? "Сдельная" : "Одной суммой") + "; " + empl.Note;
            }
            else if (doc is Surcharges s) {
                NameObj = "Для " + s.EmployeeName;
                Price = 0;
                Quantity = 0;
                Amount = s.Amount;
                UpRaise = s.UpRaise;
                TypeObj = Доплата;
                Comment = s.Info;
                CubatureAll = 0;
            }
            else if (doc is Pseudonym p)
            {
                NameObj = p.Product.NameItem;
                Price = p.PriceCng;
                CubatureAll = p.CubatureAll;
                Quantity = Math.Round( p.SelectedCub /p.Product.OnePice, 0);
                Amount = p.Amount;
                UpRaise = 0;
                TypeObj = Пиломатериалы;
                Comment = p.Operation.ToString();
            }
            else  throw new NotImplementedException("DependentCode: Отуствие класса или структуры " + doc.ToString());
        }

    }
}
