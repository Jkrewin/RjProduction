
using System.Diagnostics;

namespace RjProduction.Model.DocElement
{
    /// <summary>
    /// Сотрудник
    /// </summary>
    public struct Employee : IDoc, DocRow.IDocRow
    {
        public double UpRaise { get; set; }
        public string NameEmployee { get; set; }
        public string Note { get; set; }
        public double Payment { get; set; }
        public bool Worker { get; set; }
        public readonly decimal Amount { get => (decimal)Payment + (decimal)UpRaise; }
        public readonly float CubatureAll => 0;

        public string Name_last { get; set; }
        public string Name_first { get; set; }
        public string Name_middle { get; set; }

        public void CreateFullName(string last, string first, string middle)
        {
            if (string.IsNullOrEmpty(Name_last) == false & string.IsNullOrEmpty(Name_first) == false & string.IsNullOrEmpty(Name_middle) == false)
            {
                NameEmployee = string.Concat(last, first.AsSpan(0, 1), middle.AsSpan(0, 1));
            }
        }

        public void Send_DB(string id)
        {
           
        }

        public DocRow ToDocRow(string grupname, string id_document)
        {
            return new(id_document, grupname, "Оплата: " + (Worker ? "Сдельная" : "Одной суммой") + "; " + Note, NameEmployee, Payment, 0, Amount, DocRow.Сотрудники, UpRaise, CubatureAll);
        }

        public override readonly string ToString() => NameEmployee + " / " + Amount.ToString() + "руб" + (Worker == false ? "." : "*") + (UpRaise == 0 ? "" : " +" + UpRaise.ToString() + "p.");
    }
}
