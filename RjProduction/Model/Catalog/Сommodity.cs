using RjProduction.Model.Classifier;
using RjProduction.Sql;

namespace RjProduction.Model.Catalog
{
    /// <summary>
    /// Товар обобщенный класс как вид общего товара может быть любым товаром
    /// </summary>
    public class Сommodity : SqlParam
    {
        private string _Country = "";
        private CountryOKSM _CountryOKSM;

        /// <summary>
        /// Наименование товара
        /// </summary>
        public required string Goods_Name { get; set; }
        /// <summary>
        /// код товара или артикул
        /// </summary>
        public string? Article { get; set; }
        /// <summary>
        /// Упаковка для этого товара и ед измер
        /// </summary>
        public PackagingClass Packaging { get; set; } = new();
        /// <summary>
        /// Количество
        /// </summary>
        public double Quantity { get; set; }
        /// <summary>
        /// Цена
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// процент ндс 10, 20
        /// </summary>
        public byte NDS_Rate { get; set; }
        /// <summary>
        /// Сумма без  учета НДС, руб. коп
        /// </summary>
        [SqlIgnore] public double Amount { get => Quantity * Price; }
        /// <summary>
        /// Cумма, руб. коп НДС
        /// </summary>
        [SqlIgnore] public double OnlyNDSAmount { get => Amount * (NDS_Rate / 100); }
        /// <summary>
        /// Сумма с учетом НДС,руб.коп
        /// </summary>
        [SqlIgnore] public double AmountNDS { get => Amount + OnlyNDSAmount; }
        /// <summary>
        /// На этом складе товар
        /// </summary>
        public WarehouseClass? Warehouse { get; set; }
        /// <summary>
        /// Страна происхождения товара 
        /// </summary>
        public CountryOKSM Country
        {
            get => _CountryOKSM;
            set
            {
                _CountryOKSM = value;
                _Country = _CountryOKSM.ShortName;
            }
        }
        /// <summary>
        /// Группа товаров
        /// </summary>
        public GrupGoods Grup { get; set; } = new();
        /// <summary>
        /// Штрих код товара
        /// </summary>
        public string Barcode { get; set; } = "";
        
        /// <summary>
        /// Название Страны
        /// </summary>
        public string NameCountry
        {
            get
            {
                if (Country.IsDefault) return _Country;
                else return Country.ShortName;
            }
            set => _Country = value;
        }


        /// <summary>
        /// Класс упаковки
        /// </summary>
        public class PackagingClass : SqlFlat
        {
            private string _unit = "";
            private UnitMeasurement _UnitMeas;
            /// <summary>
            /// Наименование упаковки
            /// </summary>
            public UnitMeasurement UnitMeas
            {
                get => _UnitMeas;
                set
                {
                    _UnitMeas = value;
                    _unit = _UnitMeas.Name;
                }
            }
            /// <summary>
            /// Название единици измерения
            /// </summary>
            public string NameUnit
            {
                get
                {
                    if (UnitMeas.IsDefault) return _unit;
                    else return UnitMeas.Name;
                }
                set => _unit = value;
            }
            /// <summary>
            /// Тип упаковки
            /// </summary>
            public string Type_Packaging { get; set; } = "";
            /// <summary>
            ///  Количество в одном  месте
            /// </summary>
            public double Quantity_Info_One { get; set; }
            /// <summary>
            ///  Количество мест, штуk
            /// </summary>
            public double Quantity_Info_Place { get; set; }
            /// <summary>
            ///  Масса брутто
            /// </summary>
            public double Weight { get; set; }
        }
    }
}
