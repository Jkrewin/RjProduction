
namespace RjProduction.Model.DocElement
{
    /// <summary>
    /// Таблица для учета бревен по штучно
    /// </summary>
    public class Tabel_Timbers : IDoc, IConvertDoc
    {
        public List<Timber> Timbers = [];
        public decimal Amount
        {
            get
            {
                return (decimal)(from tv in Timbers select (tv.Куб_М * (tv.Цена + UpRaise))).Sum(x => x);
            }
        }
        public double CubatureAll { get => Timbers.Sum(x => x.Куб_М); }
        public double UpRaise { get; set; }
        public TypeWoodEnum TypeWood { get; set; } = TypeWoodEnum.Любой;

        public Products ToProducts()
        {
            return new Products()
            {
                NameItem = "Круглый лес (" + TypeWood.ToString() + ")",
                OnePice = 0,
                Cubature = CubatureAll,
                TypeWood = TypeWoodEnum.Любой,
                Price = (Timbers.Sum(x => x.Цена) / Timbers.Sum(x => x.Количество))
            };
        }

        public override string ToString() => "Бревен: " + Timbers.Sum(x => x.Количество) + " Куб/м " + Math.Round(Timbers.Sum(x => x.Куб_М), 3) + (UpRaise == 0 ? "" : " + к цене по итогу:" + UpRaise + "p.");

        /// <summary>
        /// Бревна для таблици 
        /// </summary>
        public class Timber : ICloneable
        {
            public int Диаметр { get; set; }
            public double Длинна { get; set; }
            public int Количество { get; set; }
            public double Куб_М { get; set; }
            public double Цена { get; set; }
            public decimal Сумма { get => (decimal)(Куб_М * (Цена + UpRaise)); }
            public double UpRaise { get; set; }

            public object Clone() => MemberwiseClone();

        }
    }
}
