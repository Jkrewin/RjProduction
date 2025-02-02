

using RjProduction.Sql;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;

namespace RjProduction.Model
{
    /// <summary>
    /// Статусы документа
    /// </summary>
    public enum StatusEnum
    {
        Не_Проведен = 0, Проведен = 1, Ошибка = 6, Частично = 2, НеСохранен = 4
    }

    /// <summary>
    /// Структура для объектов документа
    /// </summary>
    public interface IDoc
    {
        public decimal Amount { get; }
        public double CubatureAll { get; }
    }

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

        public Products ToProducts() {
            return new Products()
            {
                NameItem = "Круглый лес ("+ TypeWood.ToString()+")",
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
    /// <summary>
    /// Група для IDoc
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

        public override readonly string ToString() => NameEmployee + " / " + Amount.ToString() + "руб" + (Worker == false ? "." : "*") + (UpRaise == 0 ? "" : " +" + UpRaise.ToString() + "p.");
    }
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
    /// <summary>
    /// Авто транспорт
    /// </summary>
    public struct Track : IDoc
    {
        public string TrackName;
        public string TrailerName;

        public readonly double CubatureAll => 0;
        public readonly decimal Amount => 0;

        public double UpRaise { readonly get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
    /// <summary>
    /// Общая форма материалов необходимы для учета в общей бд
    /// </summary>
    public class Pseudonym : IDoc
    {
        /// <summary>
        /// Продукт  обрабатываеммый класс
        /// </summary>
        public required Products Product;

        /// <summary>
        /// Цена за один куб. Это цена своя а не из Products, измененная цена
        /// </summary>
        public double PriceCng;
        /// <summary>
        /// Выбранная куботура для этого Product
        /// </summary>
        public required double SelectedCub;
        /// <summary>
        /// Операция с кубами
        /// </summary>
        public SqlRequest.OperatonEnum Operation = SqlRequest.OperatonEnum.vsNone;
        /// <summary>
        /// Строка id которая  к этому псевдониму для изменении
        /// </summary>
        public long ID_Prod { get => Product.ID; }
        /// <summary>
        /// Сумма по Pseudonym измененная а не Product там своя 
        /// </summary>
        public decimal Amount => (decimal)(SelectedCub * PriceCng);
        /// <summary>
        /// Проводит расчет кубатуры в итоге из SelectedCub и Product.Cubature
        /// </summary>
        public double CubatureAll
        {
            get
            {
                if (Operation == SqlRequest.OperatonEnum.vsPlus) return SelectedCub + Product.Cubature;
                else if (Operation == SqlRequest.OperatonEnum.vsMunis) return Product.Cubature - SelectedCub;
                else if (Operation == SqlRequest.OperatonEnum.vsMutation) return SelectedCub;
                else return Product.Cubature;
            }
        }
               
    }

    /// <summary>
    /// Типы пород дерева
    /// </summary>
    public enum TypeWoodEnum: int
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
    /// <summary>
    /// Коды документов
    /// </summary>
    public struct DocCode
    {
        public const string Общий_Тип = "";
        public const string Производство_Cклад = "01А02";
        public const string Выравнивание_Остатков = "03A01";
        public const string Перемещение_По_Складам = "03A02";

        /// <summary>
        /// Выгрущить все названия документов в массив
        /// </summary>
        /// <returns></returns>
        public static string[] ToArray() =>
            typeof(DocCode).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Select(x => x.Name.Replace("_", " ")).ToArray();
        /// <summary>
        /// Получить код по названию документа
        /// </summary>
        /// <param name="const_code">текст проходит фильтер на замену пробелов на символ _</param>
        /// <returns></returns>
        public static string ToCode(string const_code)
        {
            FieldInfo[] codes = typeof(DocCode).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var code in codes)
            {
                if (code.Name == const_code.Replace(" ", "_"))
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
