using RjProduction.Model;
using RjProduction.Sql;
using RjProduction.Model.DocElement;
using System.Text;
using System.Windows;

namespace RjProduction.XML
{
    /// <summary>
    /// Списание продукции
    /// </summary>
    public class DocWriteDowns : XmlProtocol, IDocMain
    {
        [SqlIgnore]
        public string Doc_Code
        {
            get => DocCode.Списание_Продукции;
        }
        /// <summary>
        /// Необходимый индификатор для общей бд бля быстрого поиска этого документа
        /// </summary>
        public string ID_Doc
        {
            get => Convert.ToHexString(System.Security.Cryptography.MD5.HashData(Encoding.ASCII.GetBytes($"{Doc_Code}{DataCreate}+{Number}")));
        }
        /// <summary>
        /// Заголовок документа
        /// </summary>
        public string DocTitle { get; set; } = "Списание продукции";
        /// <summary>
        /// Склад где списание
        /// </summary>
        public WarehouseClass Warehouse { get; set; }
        /// <summary>
        /// Причина списание
        /// </summary>
        public string Reason { get; set; }

        public double AllCubs
        {
            get
            {
                double d = 0;
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

        public DocWriteDowns()
        {
            TypeDoc = Doc_Code + DocTitle;
            Warehouse = new WarehouseClass() { NameWarehouse = "n/a" };
            Reason = "Уничтожение товара";
        }

        /// <summary>
        /// Провести документ 
        /// </summary>
        public void CarryOut()
        {
            if (Status != StatusEnum.Не_Проведен) {
                MessageBox.Show("Текущий статус документа: " + Status.ToString());
                return;
            } 
            if (MDL.SqlProfile == null)
            {
                IDocMain.ErrorMessage(IDocMain.Error_Txt.Нет_подключенияБД);
                return;
            }
            if (string.IsNullOrEmpty(Reason) )
            {
                MessageBox.Show("Не указана причина списания");
                return;
            }
            var id_doc = SqlRequest.ExistRecord<DocWriteDowns>(new ISqlProfile.FieldSql("ID_Doc", ID_Doc));
            if (id_doc != -1)
            {
                MessageBox.Show("Этот документ был ранее проведен или произошла ошибка. С такой датой и номером уже ранее сохранен в БД. ");
                Status = StatusEnum.Проведен;
                XmlProtocol.SaveDocXml<DocWriteDowns>(this);
                return;
            }

            Warehouse.SyncClass();
          
            // запуск сохранения в БД Products              
            foreach (var item in ListPseudonym)
            {
                Products? cl = SqlRequest.ReadData<Products>([new("NameItem", item.Product.NameItem), new("Warehouse", item.Product.Warehouse.ID)]);
                // Меняет текущее в на этом складе и по этому названию 
                if (cl != null)
                {
                    cl.ConcurrentReqest(MDL.SqlProfile, SqlRequest.OperatonEnum.vsMunis, item.SelectedCub);
                    item.Product .SyncError = cl.SyncError; // сохранить ошибку если была
                }
                // создает новое значение если не найдено 
                else SqlRequest.SetData(item.Product);
            }

            // далее сохранения документа в БД document
            SqlRequest.SetData(this);
           
            Status = StatusEnum.Проведен;
            var products = (from tv in ListPseudonym where tv is IConvertDoc select ((IConvertDoc)tv).ToProducts()).ToArray<Products>();
            foreach (var obj in MainTabel)
            {
                foreach (var tv in obj.Tabels)
                {
                    if (tv is IConvertDoc doc)
                    {
                        // Если транзакция была проблем сохраняет в этот документ если были ошибки то частично проведен 
                        var p = doc.ToProducts();
                        if (products.Any(x => x.NameItem == p.NameItem & p.SyncError == true))
                        {
                            Status = StatusEnum.Частично;
                            continue;
                        }
                    }
                    if (tv is DocRow.IDocRow d)
                    {
                        SqlRequest.SetData(d.ToDocRow(obj.NameGrup, ID_Doc));
                        d.Send_DB(this,obj); // Дополнительная отправка в дб строики если нужно 
                    }
                    else throw new NotImplementedException("DependentCode: Отуствие класса или структуры " + tv.ToString() + "\n ()CarryOut " + this.ToString());
                }
            }
          
            XmlProtocol.SaveDocXml<DocWriteDowns>(this);
        }
    }
}
