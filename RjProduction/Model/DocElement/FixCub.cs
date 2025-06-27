

namespace RjProduction.Model.DocElement
{
    public class FixCub : IDoc
    {
        public double Price { get; set; }
        public double Plan { get; set; } = 0;
        public double Fact { get; set; }

        public decimal Amount => (decimal)(Price * Fact);
        public double CubatureAll => Fact;

        public override string ToString() =>"Фактически: " + CubatureAll + " м/куб = " + Amount + " руб.";
    }
}
