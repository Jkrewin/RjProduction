
using RjProduction.Sql;
using System.Data;

namespace RjProduction.Model
{
    /// <summary>
    /// Общая продукция для учета в общей БД
    /// </summary>
    public class Products : SqlParam, ICloneable, IConcurrentReqest
    {
        private double _Cubature;
        bool _SyncError = false;

        /// <summary>
        /// Название пиломатериала.
        /// </summary>
        public string NameItem { get; set; } = "def name";
        /// <summary>
        /// Общая Кубатура
        /// </summary>
        public double Cubature
        {
            get => _Cubature;
            set => _Cubature = value;            
        }
        /// <summary>
        /// На этом складе продукция
        /// </summary>
        public WarehouseClass Warehouse { get; set; } = new WarehouseClass();
        /// <summary>
        /// Тип леса
        /// </summary>
        public TypeWoodEnum TypeWood { get; set; } = TypeWoodEnum.Любой;
        /// <summary>
        /// Кубатура одной штуки (нужен для штучного расчета)
        /// </summary>
        public double OnePice { get; set; }
        /// <summary>
        /// Цена за один куб
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// Сумма
        /// </summary>
        [SqlIgnore] public decimal Amount => (decimal)(Price * _Cubature);
        /// <summary>
        /// Ошибка при транзакции 
        /// </summary>
        [SqlIgnore] public bool SyncError { get=> _SyncError; }

        public Products() {
            // необходим для авто создание SqlRequest
        }

        public Products(DataRow dataRow, Dictionary<long, WarehouseClass> WarehouseHub)
        {
            OnePice = 0;
            if (double.TryParse(dataRow[nameof(OnePice)].ToString(), out double dou)) OnePice = dou;
            NameItem = dataRow[nameof(NameItem)].ToString() ?? "def name";
            FullSet(dataRow);

            if (double.TryParse(dataRow[nameof(Cubature)].ToString(), out double d)) _Cubature = d;
            if (int.TryParse(dataRow[nameof(TypeWood)].ToString(), out int i)) TypeWood = (TypeWoodEnum)i;
            if (double.TryParse(dataRow[nameof(Price)].ToString(), out double dd)) Price = dd;

            if (long.TryParse(dataRow[nameof(Warehouse)].ToString(), out long w))
            {
                if (WarehouseHub.TryGetValue(w, out WarehouseClass? value)) Warehouse = value;
            }

        }

        public object Clone()
        {
            long id = this.ID;
            this.IDField = -1;

            Products products = (Products)MemberwiseClone();
            this.IDField = id;
            return products;
        }

        public void ConcurrentReqest(ISqlProfile sqlProfile, SqlRequest.OperatonEnum operaton, double value)
        {
            double d = ((IConcurrentReqest)this).ReqestTransaction(sqlProfile, TabelName, nameof(Cubature), value, ID, operaton);
            _SyncError = d == -1;
            if (_SyncError == false) _Cubature = d;
        }


    }
}
