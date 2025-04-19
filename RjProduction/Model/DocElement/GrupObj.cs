
namespace RjProduction.Model.DocElement
{
    /// <summary>
    /// Группа для IDoc
    /// </summary>
    public class GrupObj
    {
        public required string NameGrup { get; set; }
        public List<IDoc> Tabels { get; set; } = [];
        public decimal Amount { get => Tabels.Sum(x => x.Amount); }
        public double Cubature { get => Tabels.Sum(x => x.CubatureAll); }

        public override string ToString()
        {
            return NameGrup;
        }

    }
}
