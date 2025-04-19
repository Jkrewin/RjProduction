
namespace RjProduction.Model.DocElement
{
    /// <summary>
    /// Доплаты рабочим
    /// </summary>
    public struct Surcharges : IDoc
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
        public readonly double CubatureAll => 0;

        public override readonly string ToString()
        {
            if (TypeSurcharges == TypeSurchargesEnum.ДоплатаСумме) return "Долачивать всем к зарплате: +" + UpRaise.ToString();
            else if (TypeSurcharges == TypeSurchargesEnum.ДоплатаЦене) return "Увеличить цену на " + UpRaise.ToString();
            else return "Без доплат";
        }
    }
}
