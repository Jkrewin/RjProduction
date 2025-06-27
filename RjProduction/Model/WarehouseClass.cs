using RjProduction.Model.Catalog;
using RjProduction.Sql;
using System.Data;
using System.Windows;
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
        private AddresStruct _InfoWarehouse = new ();

        private static string GetDate() => DateTime.Now.TimeOfDay.ToString();

        public string NameWarehouse { get => _NameWarehouse; set { _NameWarehouse = value; _SyncData = GetDate(); } }
        public string DescriptionWarehouse { get => _DescriptionWarehouse; set { _DescriptionWarehouse = value; SyncData = GetDate(); } }
        [SqlIgnore]public AddresStruct InfoWarehouse { get => _InfoWarehouse; set { _InfoWarehouse = value; SyncData = GetDate(); } }
        /// <summary>
        /// Синхронизирует справочники если были изменения с общей БД, так как локально могут храниться эта инфа
        /// </summary>
        public string SyncData { get => _SyncData; set => _SyncData = value; }
        [SqlIgnore, XmlElement] public long ID_XML { get => IDField; set { IDField = value; } }

        public string AddressWarehouse { get => _InfoWarehouse.Address; set => _InfoWarehouse.Address = value; }
        public string EMailWarehouse { get => _InfoWarehouse.Email; set => _InfoWarehouse.Email = value; }
        public string PhoneWarehouse { get => _InfoWarehouse.Phone; set => _InfoWarehouse.Phone = value; }

        public WarehouseClass() { }

        public WarehouseClass(DataRow dataRow) {
            FullSet(dataRow);             
            if (dataRow.Table.Columns.Contains(nameof(NameWarehouse))) _NameWarehouse = dataRow[nameof(NameWarehouse)].ToString()!;
            if (dataRow.Table.Columns.Contains(nameof(DescriptionWarehouse))) _DescriptionWarehouse = dataRow[nameof(DescriptionWarehouse)].ToString()!;
            if (dataRow.Table.Columns.Contains(nameof(AddressWarehouse))) AddressWarehouse = dataRow[nameof(AddressWarehouse)].ToString()!;
            if (dataRow.Table.Columns.Contains(nameof(EMailWarehouse))) EMailWarehouse = dataRow[nameof(EMailWarehouse)].ToString()!;
            if (dataRow.Table.Columns.Contains(nameof(PhoneWarehouse))) PhoneWarehouse = dataRow[nameof(PhoneWarehouse)].ToString()!;
            if (dataRow.Table.Columns.Contains(nameof(SyncData))) _SyncData = dataRow[nameof(SyncData)].ToString()!;
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

        /// <summary>
        /// Удаляет сведения из общей бд а также инфа о товаре которая хранилась на этом складе. Удаление будет отменено если товар не равен 0 
        /// </summary>
        public void Delete() {
            if (ID == -1) return;
            if (MDL.SqlProfile is null) { MessageBox.Show("Удаление склада в общей БД невозможно: профиль подключения к БД не выбран"); return; }

            MDL.SqlProfile.Conection(true);
            try
            {
                var tt = MDL.SqlProfile.GetFieldSql($"{nameof(Products.Cubature)}!=0 AND {nameof(Products.Warehouse)}={ID}", nameof(Products));
                if (tt.Length != 0)
                {
                    MessageBox.Show("Удаление склада в общей БД невозможно: на складе остались остатки ");
                }
                else
                {
                    MDL.SqlProfile.Delete(nameof(Products), $"{nameof(Products.Warehouse)}={ID}");
                    MDL.SqlProfile.Delete(nameof(WarehouseClass), nameof(WarehouseClass.ID) + "=" + ID.ToString());
                    IDField = -1;
                }
            }
            catch (Exception ex)
            {
                MDL.SqlProfile.Transaction(ISqlProfile.TypeTransaction.roolback);
                MDL.SqlProfile.Disconnect();
                MDL.LogError(ex.Message, ex.Source ?? string.Empty);
            }
            finally
            {
                MDL.SqlProfile.Transaction(ISqlProfile.TypeTransaction.commit);
                MDL.SqlProfile.Disconnect();
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
