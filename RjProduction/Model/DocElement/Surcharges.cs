
using System.Windows.Controls;

namespace RjProduction.Model.DocElement
{
    /// <summary>
    /// Доплаты рабочим
    /// </summary>
    public struct Surcharges : IDoc, DocRow.IDocRow
    {
        public double UpRaise { get; set; }
        public string Info { get; set; }
        public TypeSurchargesEnum TypeSurcharges { get; set; }
        public string EmployeeName { get; set; }


        public enum TypeSurchargesEnum
        {
            ДоплатаСумме, ДоплатаЦене, none
        }
        public readonly decimal Amount => 0;
        public readonly float CubatureAll => 0;

        public override readonly string ToString()
        {
            if (TypeSurcharges == TypeSurchargesEnum.ДоплатаСумме) return "Долачивать всем к зарплате: +" + UpRaise.ToString();
            else if (TypeSurcharges == TypeSurchargesEnum.ДоплатаЦене) return "Увеличить цену на " + UpRaise.ToString();
            else return "Без доплат";
        }

        public DocRow ToDocRow(string grupname, string id_document)
        {
            return new(id_document, grupname,Info, "Для " + EmployeeName, 0, 0, Amount, DocRow.Доплата, UpRaise, CubatureAll);
        }

        
        public void Send_DB(IDocMain doc, GrupObj grp)
        {
           
        }
    }
}
