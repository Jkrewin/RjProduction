
using RjProduction.Sql;

namespace RjProduction.Model
{
    public sealed class Products : SqlParam
    {
        private uint _Quantity;

       
        /// <summary>
        /// Значение названия круглого леса для <i>NameItem</i>
        /// </summary>
        public readonly string NameKB = "Круглый Лес";
        /// <summary>
        /// Название пиломатериала. Для круглого леса есть переменная NameKB
        /// </summary>
        public string NameItem { get; set; } = "def name";
        /// <summary>
        /// Количество бревен или пиломатериалов не может быть отрицательным
        /// </summary>
        public uint Quantity
        {
            get => _Quantity;
            set
            {
                unchecked
                {
                    _Quantity = value;
                }
            }
        }
        /// <summary>
        /// Общая Кубатура
        /// </summary>
        public int Cubature { get; set; }
        /// <summary>
        /// На этом складе продукция
        /// </summary>
        public WarehouseClass Warehouse { get; set; } = new WarehouseClass();

    }
}
