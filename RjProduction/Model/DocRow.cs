

namespace RjProduction.Model
{
    public class DocRow: Sql.SqlParam
    {       
        public long ID_Doc { get; set; }
        public string NameObj = "";
        public decimal Amount;
        public double Quantity;
        public double Price;
        public string TypeObj="";
        public string GrupName = "";
        public string Comment = "";
        public double UpRaise;

        public DocRow() { }

        public DocRow(IDoc doc, string grupname, long id_document) {
            ID_Doc = id_document;
            GrupName = grupname;
            if (doc is MaterialObj material)
            {
                NameObj = material.NameMaterial;
                Price = material.Price;
                Quantity = material.Quantity;
                Amount = material.Amount;
                TypeObj = "A06:Пиломатериалы";
                UpRaise = material.UpRaise;
            }
            else if (doc is Tabel_Timbers timber)
            {
                NameObj = Products.TypeWoodStr(timber.TypeWood);
                Price = -1;
                Quantity = timber.Timbers.Sum(x => x.Количество);
                Amount = timber.Amount;
                TypeObj = "A05:Круглый лес";
                UpRaise = timber.UpRaise;
            }
            else if (doc is Employee empl)
            {
                NameObj = empl.NameEmployee;
                Price = empl.Payment;
                Quantity = 0;
                Amount = empl.Amount;
                TypeObj = "A04:Сотрудники";
                UpRaise = empl.UpRaise;
                Comment = "Оплата: " + (empl.Worker ? "Сдельная" : "Одной суммой") + "; " + empl.Note;
            }
            else if (doc is Surcharges s) {
                NameObj = "Для " + s.EmployeeName;
                Price = 0;
                Quantity = 0;
                Amount = s.Amount;
                UpRaise = s.UpRaise;
                TypeObj = "B01:Доплата";
                Comment = s.Info;
            }
        }
    }
}
