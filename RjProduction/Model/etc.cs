

using RjProduction.Sql;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using static RjProduction.Model.Products;

namespace RjProduction.Model
{
    
    public enum StatusEnum
    {
        Не_Проведен = 0, Проведен = 1, Ошибка = 6, Частично =2
    }

    public interface IDoc
    {
        public decimal Amount { get; }
        public double CubatureAll { get; }
    }

    public class Tabel_Timbers : IDoc
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

        public override string ToString() => "Бревен: " + Timbers.Sum(x => x.Количество) + " Куб/м " + Math.Round(Timbers.Sum(x => x.Куб_М), 3) + (UpRaise == 0 ? "" : " + к цене по итогу:" + UpRaise + "p.");
    }

    public class GrupObj
    {
        public required string NameGrup { get; set; }
        public List<IDoc> Tabels { get; set; } = [];
        public decimal Amount { get => Tabels.Sum(x => x.Amount); }
        public double Cubature { get => Tabels.Sum(x => x.CubatureAll); }
    }

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

    public struct Employee : IDoc
    {
        public double UpRaise { get; set; }
        public string NameEmployee { get; set; }
        public string Note { get; set; }
        public double Payment { get; set; }
        public bool Worker { get; set; }
        public readonly decimal Amount { get => (decimal)Payment + (decimal)UpRaise; }
        public readonly double CubatureAll => 0;

        public override readonly string ToString() => NameEmployee + " / " + Amount.ToString() + "руб" + (Worker == false ? "." : "*") + (UpRaise == 0 ? "" : " +" + UpRaise.ToString() + "p.");
    }

    public struct MaterialObj : IDoc
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

    public struct Track : IDoc
    {
        public string TrackName;
        public string TrailerName;

        public readonly double CubatureAll => 0;
        public readonly decimal Amount => 0;

        public double UpRaise { readonly get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public class Pseudonym : IDoc {
        public required string Name;
        public double Price;
        public double OnePice;
        public required double CubAll;
        public  double CubatureAll => CubAll;
        public decimal Amount  {get=>(decimal) (CubAll* Price);}
        /// <summary>
        /// true - означает вычитать из общего баланса
        /// </summary>
        public SqlRequest.OperatonEnum Operation;
        /// <summary>
        /// Строка id которая  к этому псевдониму для изменении
        /// </summary>
        public required long ID_Prod;
        /// <summary>
        /// Была ошибка при синхронизации
        /// </summary>
        public bool SyncError = false;
    }

    public enum TypeWoodEnum
    {
        Любой,
        Хвоя,
        Листва,
        Сосна,
        Береза,
        Осина,
        Дуб,
        Ель,
        Пихта,
        Лиственица,
        Кедр,
        Ясень,
        Клен,
        Вяз, 
        Тополь,
        Ольха,
        Липа,
        Ива
    }
        
    public struct DocCode {
        public const string Общий_Тип = "";
        public const string Производство_склад = "01А02";
        public const string ВыравниваниеОстатков = "03A01";

        public static string[] ToArray() => 
            typeof(DocCode).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Select(x => x.Name.Replace("_"," " )).ToArray();

        public static string ToCode(string const_code) {
            FieldInfo[] codes = typeof(DocCode).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var code in codes)
            {
                if (code.Name == const_code.Replace(" ","_"))
                {
                    object? obj = code.GetValue(null);
                    if (obj is not null) return obj.ToString() ?? string.Empty;
                    else break;
                }
            }
            return string.Empty;
        }
    }

    
}
