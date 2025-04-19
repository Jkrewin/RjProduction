
using System.Diagnostics.CodeAnalysis;

namespace RjProduction.Model.DocElement
{
    /// <summary>
    /// Пиломатериал
    /// </summary>
    public struct MaterialObj : IDoc, IConvertDoc
    {
        public TypeWoodEnum TypeWood { get; set; }
        /// <summary>
        /// Куботура одной доски
        /// </summary>
        public readonly double Cub
        {
            get
            {
                if (MaterialType == MaterialTypeEnum.Количество) return (WidthMaterial / 1000) * (HeightMaterial / 1000) * (LongMaterial / 1000);
                else return Math.Round((WidthMaterial / 100) * (HeightMaterial / 100) * (LongMaterial / 1000) * (Ratio / 100), 3);
            }
        }
        public double WidthMaterial;
        public double HeightMaterial;
        public double LongMaterial;
        public double Quantity;
        public MaterialTypeEnum MaterialType { get; set; }
        /// <summary>
        /// общая куботура
        /// </summary>
        public readonly double CubatureAll { get => Cub * Quantity; }
        public double Price;
        public double UpRaise { get; set; }
        public readonly decimal Amount { get => (decimal)((Price + UpRaise) * CubatureAll); }
        /// <summary>
        /// коэффициент для н/о
        /// </summary>
        public double Ratio;

        public readonly string NameMaterial
        {
            get
            {
                if (MaterialType == MaterialTypeEnum.Количество)
                {
                    if (TypeWood == TypeWoodEnum.Любой)
                    {
                        return $"[{WidthMaterial} x {HeightMaterial} x {LongMaterial}]";
                    }
                    else
                    {
                        return $"[{WidthMaterial} x {HeightMaterial} x {LongMaterial}]({TypeWood}) ";
                    }
                }
                else return $"[{WidthMaterial} x {HeightMaterial} x {LongMaterial} x {Ratio}]* ";
            }
        }

        public readonly Products ToProducts()
        {
            string name_wood;
            if (MaterialType == MaterialObj.MaterialTypeEnum.Количество)
            {
                // обрезная доска
                name_wood = NameMaterial;
            }
            else
            {
                // тут н/о
                if (TypeWood == TypeWoodEnum.Любой) name_wood = "Необрезная " + LongMaterial.ToString() + "метровый";
                else name_wood = "Необрезная (" + TypeWood.ToString() + ")" + LongMaterial.ToString() + "метровый";
            }

            return new Products()
            {
                OnePice = Cub,
                Cubature = CubatureAll,
                NameItem = name_wood,
                TypeWood = TypeWood,
                Price = Price
            };
        }

        public enum MaterialTypeEnum
        {
            Количество, Объем, none
        }

        public override readonly string ToString()
        {
            if (UpRaise == 0)
            {
                return NameMaterial + " " + Math.Round(CubatureAll, 3) + "M3 " + Quantity + "шт. " + Price + "p. = " + Amount + "p.";
            }
            else
            {
                return NameMaterial + " " + Math.Round(CubatureAll, 3) + "M3 " + Quantity + "шт. " + Price + "p. (+" + UpRaise + ") = " + Amount + "p.";
            }
        }

        public static bool operator ==(MaterialObj obj1, MaterialObj obj2)
        {
            return obj1.WidthMaterial == obj2.WidthMaterial & obj1.HeightMaterial == obj2.HeightMaterial & obj1.LongMaterial == obj2.LongMaterial;
        }

        public static bool operator !=(MaterialObj obj1, MaterialObj obj2)
        {
            return !(obj1.WidthMaterial != obj2.WidthMaterial || obj1.HeightMaterial != obj2.HeightMaterial || obj1.LongMaterial != obj2.LongMaterial);
        }

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => base.Equals(obj);
        public override readonly int GetHashCode() => base.GetHashCode();
    }
}
