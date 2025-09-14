using System.Diagnostics.CodeAnalysis;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace RjProduction.Model.DocElement
{
    /// <summary>
    /// Пиломатериал
    /// </summary>
    public struct MaterialObj : IDoc, IConvertDoc, DocRow.IDocRow
    {
        public TypeWoodEnum TypeWood { get; set; }
        public double WidthMaterial;
        public double HeightMaterial;
        public double LongMaterial;
        public double Quantity;
        public MaterialTypeEnum MaterialType { get; set; }
        public double Price;
        public double UpRaise { get; set; }
        public double Ratio;
        public GradeEnum Grade;

        // Явно объявленный конструктор
        public MaterialObj(TypeWoodEnum typeWood, double widthMaterial, double heightMaterial, double longMaterial, double quantity, MaterialTypeEnum materialType, double price, double upRaise, double ratio, GradeEnum grade)
        {
            TypeWood = typeWood;
            WidthMaterial = widthMaterial;
            HeightMaterial = heightMaterial;
            LongMaterial = longMaterial;
            Quantity = quantity;
            MaterialType = materialType;
            Price = price;
            UpRaise = upRaise;
            Ratio = ratio;
            Grade = grade;
        }

        public readonly double Cub
        {
            get
            {
                if (MaterialType == MaterialTypeEnum.Количество) return (WidthMaterial / 1000) * (HeightMaterial / 1000) * (LongMaterial / 1000);
                else return Math.Round((WidthMaterial / 100) * (HeightMaterial / 100) * (LongMaterial / 100) * (Ratio / 100), 3);
            }
        }

        public readonly float CubatureAll { get => (float)(Cub * Quantity); }
        public readonly decimal Amount { get => (decimal)((Price + UpRaise) * CubatureAll); }

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
            if (MaterialType == MaterialTypeEnum.Количество)
            {
                name_wood = NameMaterial;
            }
            else
            {
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

        public enum GradeEnum
        {
            Нет, I, II, III, IV, I_II_III, I_II
        }

        public override readonly string ToString()
        {
            string g;
            if (Grade == GradeEnum.Нет) g = "";
            else g = Grade.ToString().Replace("_", ",");

            if (UpRaise == 0)
            {
                return NameMaterial + " " + g + " " + Math.Round(CubatureAll, 3) + "M3 " + Quantity + "шт. " + Price + "p. = " + Math.Round( Amount,2) + "p.";
            }
            else
            {
                return NameMaterial + " " + g + " " + Math.Round(CubatureAll, 3) + "M3 " + Quantity + "шт. " + Price + "p. (+" + UpRaise + ") = " + Math.Round(Amount, 2) + "p.";
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

        public DocRow ToDocRow(string grupname, string id_document) 
        {                      
            return new(id_document, grupname, string.Empty, NameMaterial, Price, Quantity, Amount, DocRow.Пиломатериалы, UpRaise, CubatureAll);
        }

        public void Send_DB(IDocMain doc, GrupObj grp)
        {
          
        }




    }
}
