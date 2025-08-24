

namespace RjProduction.Model.DocElement
{
    public class FixCub : IDoc
    {
        public double Price { get; set; }
        public double Plan { get; set; } = 0;
        public float Fact { get; set; }

        public decimal Amount => (decimal)(Price * Fact);
        public float CubatureAll => Fact;

        public override string ToString() =>"Фактически: " + CubatureAll + " м/куб = " + Amount + " руб.";
    }
}
