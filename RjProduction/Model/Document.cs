using System.Diagnostics.CodeAnalysis;

namespace RjProduction.Model
{
    public sealed class Document
    {
        private uint _number = 0;

        /// <summary>
        /// Версия документиа
        /// </summary>
        public static string VerDoc { get => "v.1.0"; }
        /// <summary>
        /// Дата создания документа
        /// </summary>
        public DateOnly DataCreate { get; set; } = DateOnly.FromDateTime (DateTime.Now);
        /// <summary>
        /// Номер документа, также сохраняет последний номер в MDL
        /// </summary>
        public uint Number
        {
            get => _number;
            set
            {
                _number = value;
                MDL.MyDataBase.NumberDef = value +1;
            }
        }
        /// <summary>
        /// Заголовок документа
        /// </summary>
        public string DocTitle { get; set; } = ""; 
        /// <summary>
        /// Главная  таблица
        /// </summary>
        public List<GrupObj> MainTabel { get; set; } = [];
        /// <summary>
        /// Дата создания документа в строковом формате
        /// </summary>
        public string DateStr { get => DataCreate.ToString("dd.MM.yyyy"); }
        /// <summary>
        /// Текущий склад
        /// </summary>
        public WarehouseClass? Warehouse { get; set; }
        /// <summary>
        /// Тип документа
        /// </summary>
        public static string TypeDoc
        {
            get
            {
               return "01А02:Производство на склад";
            }
        }
        /// <summary>
        /// Округялет суммы рабочим без копеек
        /// </summary>
        public bool RoundingAmountsEmpl { get; set; } = true;
        /// <summary>
        /// Статус документа
        /// </summary>
        public StatusEnum Status { get; set; } = StatusEnum.Не_Проведен;
        /// <summary>
        /// Необходимый индификатор для общей бд бля быстрого поиска этого документа
        /// </summary>
        public string ID_Doc { get => $"{Number}{DataCreate.ToLongDateString}"; }

        /// <summary>
        /// Провести документ 
        /// </summary>
        public void CarryOut() { 
            
        }


        public enum StatusEnum
        {
            Не_Проведен = 0, Проведен = 1
        }

        public interface IDoc
        {
            public decimal Amount { get; }
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
            public double CubAll { get => Timbers.Sum(x => x.Куб_М); }
            public double UpRaise { get ; set ; }

            public override string ToString() => "Бревен: " + Timbers.Sum(x => x.Количество) + " Куб/м " + Math.Round(Timbers.Sum(x => x.Куб_М),3) + (UpRaise==0 ? "":" + к цене по итогу:" + UpRaise + "p.");
        }

        public class GrupObj 
        {
            public required string NameGrup { get; set; }
            public List<IDoc> Tabels { get; set; } = [];
            public decimal Amount { get => Tabels.Sum(x => x.Amount); }

        }

        public struct Surcharges : IDoc
        {
            public double UpRaise { get; set; }
            public string Info { get; set; }
            public TypeSurchargesEnum TypeSurcharges { get; set; }
            public string EmployeeName { get; set; }


            public enum TypeSurchargesEnum { 
                ДоплатаСумме, ДоплатаЦене, none
            }
            public readonly decimal Amount => 0;

            public override readonly string ToString() {
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

            public override readonly string ToString() => NameEmployee + " / " + Amount.ToString() + "руб" + (Worker == false ? "." : "*") + (UpRaise == 0 ? "" : " +" + UpRaise.ToString() + "p.");
        }

        public struct MaterialObj : IDoc
        {
            public readonly double Cub
            {
                get
                {
                    if (MaterialType == MaterialTypeEnum.Количество) return (WidthMaterial /1000) * (HeightMaterial /1000) * (LongMaterial / 1000);
                    else return Math.Round((WidthMaterial / 100) * (HeightMaterial / 100) * (LongMaterial / 1000), 3);
                }
            }
            public double WidthMaterial;
            public double HeightMaterial;
            public double LongMaterial;
            public double Quantity;
            public MaterialTypeEnum MaterialType { get; set; } 
            public readonly double CubM { get => Cub * Quantity; }
            public double Price;
            public double UpRaise { get; set; }
            public readonly decimal Amount { get => (decimal)((Price+ UpRaise) * CubM); }

            public readonly string NameMaterial
            {
                get
                {
                    if (MaterialType == MaterialTypeEnum.Количество) return $"[{WidthMaterial} x {HeightMaterial} x {LongMaterial}]";
                    else return $"[{WidthMaterial} x {HeightMaterial} x {LongMaterial}]*";
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
                    return NameMaterial + " " + Math.Round(CubM, 3) + "M3 " + Quantity + "шт. " + Price + "p. = " + Amount + "p.";
                }
                else
                {
                    return NameMaterial + " " + Math.Round(CubM, 3) + "M3 " + Quantity + "шт. " + Price + "p. (+" + UpRaise + ") = " + Amount + "p.";
                }
            }

            public static bool operator ==(MaterialObj obj1, MaterialObj obj2)
            {
                return obj1.WidthMaterial==obj2 .WidthMaterial & obj1.HeightMaterial ==obj2 .HeightMaterial & obj1 .LongMaterial == obj2 .LongMaterial;
            }

            public static bool operator !=(MaterialObj obj1, MaterialObj obj2)
            {
                return !(obj1.WidthMaterial != obj2.WidthMaterial || obj1.HeightMaterial != obj2.HeightMaterial || obj1.LongMaterial != obj2.LongMaterial);
            }

            public override readonly bool Equals([NotNullWhen(true)] object? obj)=> base.Equals(obj);
            public override readonly int GetHashCode() => base.GetHashCode();
        }

        public struct Track : IDoc
        {
            public string TrackName;
            public string TrailerName;


            public readonly decimal Amount => throw new NotImplementedException();

            public double UpRaise { readonly get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }
    }
}
