
using RjProduction.Sql;
using System.Data;

namespace RjProduction.Model
{
    public class Products : SqlParam
    {
        private double _Cubature;
        
        /// <summary>
        /// Название пиломатериала. Для круглого леса есть переменная NameKB
        /// </summary>
        public string NameItem { get; set; } = "def name";
        
        /// <summary>
        /// Общая Кубатура
        /// </summary>
        public double Cubature
        {
            get => _Cubature;
            set
            {
                unchecked
                {
                    _Cubature = value;
                }
            }
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

        public Products() { }

        public Products(DataRow dataRow, Dictionary<long, WarehouseClass> WarehouseHub)
        {           
            OnePice = 0;
            if (double.TryParse(dataRow["OnePice"].ToString(), out double dou)) OnePice = dou;
            NameItem = dataRow["NameItem"].ToString() ?? "def name";
            FullSet(dataRow);
            
            if (double.TryParse(dataRow["Cubature"].ToString(),out double d)) _Cubature = d;
            if (dataRow["TypeWood"] is TypeWoodEnum e) TypeWood = e;
            if (double.TryParse(dataRow["Price"].ToString(), out double dd)) Price = dd;

            if (long.TryParse(dataRow["Warehouse"].ToString(), out long w))
            {
                if (WarehouseHub.TryGetValue(w, out WarehouseClass? value)) Warehouse = value;
            }
        }
                
        /// <summary>
        /// Преобразует тип в текст для круглого леса как в бд название
        /// </summary>
        public static string TypeWoodStr(TypeWoodEnum type) => type switch
        {
            TypeWoodEnum.Хвоя => "Круглый лес хвойный",
            TypeWoodEnum.Листва => "Круглый лес лиственный",
            _ => "Круглый леc"

        };
      
    }
}
