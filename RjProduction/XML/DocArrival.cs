using RjProduction.Model;
using RjProduction.Model.DocElement;
using RjProduction.Sql;

namespace RjProduction.XML
{
    /// <summary>
    /// Производство Cклад
    /// </summary>
    public sealed class DocArrival : XmlProtocol, IDocMain
    {
        [SqlIgnore]public string Doc_Code { get => DocCode.Производство_Cклад; }

        /// <summary>
        /// Необходимый индификатор для общей бд бля быстрого поиска этого документа
        /// </summary>
        public string ID_Doc
        {
            get
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes($"{Doc_Code}{DataCreate}+{Number}");
                return Convert.ToHexString(System.Security.Cryptography.MD5.HashData(inputBytes));
            }
        }
        /// <summary>
        /// Заголовок документа
        /// </summary>
        public string DocTitle { get; set; } = "Производство на склад";

        /// <summary>
        /// Текущий склад
        /// </summary>
        public WarehouseClass Warehouse { get; set; }
              

        public float AllCubs
        {
            get
            {
                float d = 0;
                foreach (var obj in MainTabel)
                {
                    foreach (var tv in obj.Tabels)
                    {
                        if (tv is MaterialObj material) d += material.CubatureAll;
                        else if (tv is Tabel_Timbers timber) d += timber.CubatureAll;
                    }
                }
                return d;
            }
        }
        public decimal AllSum { get => MainTabel.Sum(x => x.Amount); }

        public DocArrival() { 
            TypeDoc = Doc_Code + DocTitle;
            Warehouse = new WarehouseClass() { NameWarehouse = "n/a" };
        }

        /// <summary>
        /// Провести документ 
        /// </summary>
        public void CarryOut()        {
            if (MDL.SqlProfile == null)
            {
                IDocMain.ErrorMessage(IDocMain.Error_Txt.Нет_подключенияБД);
                return;
            }
           
            if (Status == StatusEnum.Проведен) return;

            MDL.SqlProfile.SqlLogString.ErrorLogger = true;
            MDL.SqlProfile.SqlLogString.AddInfo = "DocArrival";

            // если была потерена информация или создан одинаковый документ проверка
            var id_doc = SqlRequest.ExistRecord<DocArrival>(new ISqlProfile.FieldSql("ID_Doc",  ID_Doc));
            if (id_doc != -1)
            {
                IDocMain.ErrorMessage(IDocMain.Error_Txt.Ошибка_в_документе);
                goto final;
            }

            Warehouse.SyncClass();

            //  Внесение в общую базу Products
            List<Products> products = [];
            foreach (var obj in MainTabel)
            {
                foreach (var tv in obj.Tabels)
                {
                    if (tv is IConvertDoc doc)
                    {
                        var p = doc.ToProducts();
                        var find = products.Find(x => p.NameItem == x.NameItem);
                        if (find is not null)
                        {
                            find.Cubature += p.Cubature;
                        }
                        else
                        {
                            p.Warehouse = Warehouse;
                            products.Add(p);
                        }

                    }
                }
            }
           
            List<SqlRequest.FieldSqlTrn> trn = [];
           
            try
            {
                // запуск сохранения в БД Products              
                foreach (var item in products)
                {
                    Products? cl = SqlRequest.ReadData<Products>([new("NameItem", item.NameItem), new("Warehouse", item.Warehouse.ID)]);
                    // Меняет текущее в на этом складе и по этому названию                     
                    if (cl != null) trn.Add(new SqlRequest.FieldSqlTrn(nameof(Products.Cubature), cl.ID, item.Cubature, SqlRequest.OperatonEnum.vsPlus));
                    // создает новое значение если не найдено 
                    else SqlRequest.SetData(item);
                }
                MDL.SqlProfile.SqlLogString.AddInfo = "Start Reqest Transaction";
                SqlRequest.ReqestTransaction(MDL.SqlProfile, nameof(Products), trn);
                MDL.SqlProfile.SqlLogString.AddInfo = "Start Doc Save";
                // далее сохранения документа в БД document
                SqlRequest.SetData(this);
                // потом сохраняем строки                
                foreach (var obj in MainTabel)
                {
                    foreach (var tv in obj.Tabels)
                    {
                        if (tv is DocRow.IDocRow d)
                        {
                            SqlRequest.SetData(d.ToDocRow(obj.NameGrup, ID_Doc)); // только добавление
                            d.Send_DB(this, obj); // Дополнительная отправка в дб в отдельно в таблицу
                        }
                        else throw new NotImplementedException("DependentCode: Отуствие класса или структуры " + tv.ToString() + "\n ()CarryOut " + this.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                MDL.LogError("Ошибка при попытки провести документ " + Doc_Code, ex.Message +" "+ ex.Source);
                Status = StatusEnum.Ошибка;
                XmlProtocol.SaveDocXml<DocArrival>(this);
                MDL.SqlProfile.SqlLogString.ErrorLogger = false;
                return;
            }

        final:
            Status = StatusEnum.Проведен;
            XmlProtocol.SaveDocXml<DocArrival>(this);
            MDL.SqlProfile.SqlLogString.ErrorLogger = false;
        }



    }
}
