
namespace RjProduction.Model.DocElement
{
    /// <summary>
    /// Сотрудник
    /// </summary>
    public struct Employee : IDoc
    {
        public double UpRaise { get; set; }
        public string NameEmployee { get; set; }
        public string Note { get; set; }
        public double Payment { get; set; }
        public bool Worker { get; set; }
        public readonly decimal Amount { get => (decimal)Payment + (decimal)UpRaise; }
        public readonly double CubatureAll => 0;

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

        public override readonly string ToString() => NameEmployee + " / " + Amount.ToString() + "руб" + (Worker == false ? "." : "*") + (UpRaise == 0 ? "" : " +" + UpRaise.ToString() + "p.");
    }
}
