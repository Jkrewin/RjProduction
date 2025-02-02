using RjProduction.Sql;
using System.Data;
using System.Xml.Serialization;

namespace RjProduction.Model
{
    /// <summary>
    /// Склад для общей БД и для локальной, с возможности синхрон данных
    /// </summary>
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
        [SqlIgnore, XmlElement] public long ID_XML { get => IDField; set { IDField = value; } }


        public WarehouseClass() { }

        public WarehouseClass(DataRow dataRow) {
            FullSet(dataRow);
            if (dataRow[nameof(NameWarehouse)] is string s) _NameWarehouse = s;
            if (dataRow[nameof(DescriptionWarehouse)] is string s1) _DescriptionWarehouse = s1;
            if (dataRow[nameof(AddressWarehouse)] is string s2) _AddressWarehouse = s2;
            if (dataRow[nameof(SyncData)] is string s3) _SyncData = s3;
        }

        /// <summary>
        /// Синхронизирует склад с внешней БД и локальной
        /// </summary>
        public void SyncClass() {
            // Проверка и создание нового склада и поиск склада из файла в локальной БД
            int w = MDL.MyDataBase.Warehouses.FindIndex(x => x.Equals(this));
            // если в локальной БД нет такого склада
            if (w == -1)
            {
                long lo = SqlRequest.ExistRecord<WarehouseClass>(new ISqlProfile.FieldSql(nameof(NameWarehouse), NameWarehouse));
                if (lo == -1)
                {
                    //то создадим его если нет и в общей БД
                    SqlRequest.SetData(this);
                }
               
                MDL.MyDataBase.Warehouses.Add(this); // новое значение в локальное БД
                MDL.SaveXml<MDL.Reference>(MDL.MyDataBase, MDL.SFile_DB);
            }
            // Создает если есть склада в локальной БД, создадим в общей  БД
            else
            {
                // Если нет в общей БД создадим
                long lo = SqlRequest.ExistRecord<WarehouseClass>(new ISqlProfile.FieldSql(nameof(ID), ID.ToString()));
                if (lo == -1)
                {
                    IDField = -1;//  Rebild
                    SqlRequest.SetData(this);
                    MDL.MyDataBase.Warehouses[w] = this; // обновим ID
                    MDL.SaveXml<MDL.Reference>(MDL.MyDataBase, MDL.SFile_DB);
                }
                // если есть в общей бд то проверим на актуальность 
                WarehouseClass ww = SqlRequest.ReadData<WarehouseClass>(MDL.MyDataBase.Warehouses[w].ID);
                if (ww.SyncData == MDL.MyDataBase.Warehouses[w].SyncData)
                {
                    // если они одинаковы в общей БД  =ок=
                }
                else
                {
                    // если есть отличте с локальной БД
                    MDL.MyDataBase.Warehouses[w] = ww;
                    MDL.SaveXml<MDL.Reference>(MDL.MyDataBase, MDL.SFile_DB);
                }
            }
        }

        public override string ToString() => _NameWarehouse;
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is WarehouseClass w) return w.NameWarehouse.Equals(NameWarehouse, StringComparison.CurrentCultureIgnoreCase);
            else return false;
        }
        public override int GetHashCode()=> base.GetHashCode();
    }
}
