
using RjProduction.Sql;
using System.Data;
using System.Globalization;

namespace RjProduction.Model
{
    /// <summary>
    /// Общая продукция для учета в общей БД
    /// </summary>
    public class Products : SqlParam, ICloneable, IConcurrentReqest
    {
        private float _Cubature;
        bool _SyncError = false;

        /// <summary>
        /// Название пиломатериала.
        /// </summary>
        public string NameItem { get; set; } = "def name";
        /// <summary>
        /// Общая Кубатура
        /// </summary>
        public float Cubature
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
        public float OnePice { get; set; }
        /// <summary>
        /// Цена за один куб
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// Сумма
        /// </summary>
        [SqlIgnore] public decimal Amount => (decimal)(Price * _Cubature);
        /// <summary>
        /// Ошибка при транзакции. true - была ошибка 
        /// </summary>
        [SqlIgnore] public bool SyncError { get => _SyncError; set => _SyncError = value; }

        public Products() {
            // необходим для авто создание SqlRequest
        }

        public Products(DataRow dataRow, Dictionary<long, WarehouseClass> WarehouseHub)
        {
            OnePice = 0;
            if (float.TryParse(dataRow[nameof(OnePice)].ToString(), CultureInfo.InvariantCulture,  out float dou)) OnePice = dou;
            NameItem = dataRow[nameof(NameItem)].ToString() ?? "def name";
            FullSet(dataRow);            
            if (float.TryParse(dataRow[nameof(Cubature)].ToString(), CultureInfo.InvariantCulture, out float d))  _Cubature = d; 
            if (int.TryParse(dataRow[nameof(TypeWood)].ToString(), out int i)) TypeWood = (TypeWoodEnum)i;
            if (double.TryParse(dataRow[nameof(Price)].ToString(), CultureInfo.InvariantCulture, out double dd)) Price = dd;

            if (long.TryParse(dataRow[nameof(Warehouse)].ToString(), CultureInfo.InvariantCulture, out long w))
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

      
        public void ConcurrentReqest(ISqlProfile sqlProfile, SqlRequest.OperatonEnum operaton, float value)
        {
            float d = ((IConcurrentReqest)this).ReqestTransaction(sqlProfile, TabelName, nameof(Cubature), value, ID, operaton);
            _SyncError = d == -1;
            if (_SyncError == false) _Cubature = d;
        }
    }
}
