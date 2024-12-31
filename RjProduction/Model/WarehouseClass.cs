using RjProduction.Sql;
using System.Data;

namespace RjProduction.Model
{
    public sealed class WarehouseClass : SqlParam
    {
        private string _SyncData = "";
        private string _NameWarehouse = "Новый склад";
        private string _DescriptionWarehouse = string.Empty;
        private string _AddressWarehouse = string.Empty;

        private static string GetDate() => DateTime.Now.TimeOfDay.ToString();

        public string NameWarehouse { get => _NameWarehouse; set { _NameWarehouse = value; _SyncData = GetDate(); } }
        public string DescriptionWarehouse { get => _DescriptionWarehouse; set { _DescriptionWarehouse = value; SyncData = GetDate(); } }
        public string AddressWarehouse { get => _AddressWarehouse; set { _AddressWarehouse = value; SyncData = GetDate(); } }
        /// <summary>
        /// Синхронизирует справочники если были изменения с общей БД, так как локально могут храниться эта инфа
        /// </summary>
        public string SyncData { get => _SyncData; set => _SyncData = value; }

        public WarehouseClass() { }

        public WarehouseClass(DataRow dataRow) {
            FullSet(dataRow);
            if (dataRow["NameWarehouse"] is string s) _NameWarehouse = s;
            if (dataRow["DescriptionWarehouse"] is string s1) _DescriptionWarehouse = s1;
            if (dataRow["AddressWarehouse"] is string s2) _AddressWarehouse = s2;
            if (dataRow["SyncData"] is string s3) _SyncData = s3;
        }

        public override string ToString() => _NameWarehouse;
    }
}
