
using RjProduction.Sql;

namespace RjProduction.Model
{
   public sealed  class WarehouseClass : SqlParam
    {
        public string NameWarehouse { get; set; } = "Новый склад";
        public string DescriptionWarehouse { get; set; } = string.Empty;
        public AddressStruct AddressWarehouse { get; set; }


    }
}
